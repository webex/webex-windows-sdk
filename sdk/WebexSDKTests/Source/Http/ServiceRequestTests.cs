using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Moq;
using WebexSDK;
using System.Threading;

namespace WebexSDK.Tests
{
    [TestClass]
    public class ServiceRequestTests
    {
        private static WebexTestFixture fixture;
        private static Webex webex;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            webex = fixture.CreateWebex();
            Assert.IsNotNull(webex);
        }


        [ClassCleanup]
        public static void ClassTearDown()
        {
            fixture = null;
            webex = null;
        }

        [TestMethod]
        public void ExecuteAuthResponse429TooManyRequestsTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<JWTAccessTokenInfo>();

            var request = ServiceRequest429ResponseConfig(5, 2);

            DateTime time1 = DateTime.UtcNow;
            DateTime time2 = time1;
            request.ExecuteAuth<JWTAccessTokenInfo>((r) =>
            {
                time2 = DateTime.UtcNow;
                response = r;
                completion.Set();
            });


            if (!completion.WaitOne(30000))
            {
                Console.WriteLine("ExecuteAuth timeout");
                Assert.Fail();
                return;
            }

            Console.WriteLine($"request  time: {time1}");
            Console.WriteLine($"response time: {time2}");
            Assert.IsFalse(response.IsSuccess);
            Assert.IsTrue(request.m429RetryCount == 5);
            Assert.IsTrue((int)(time2-time1).TotalSeconds == 5*2);
        }

        [TestMethod]
        public void ExecuteAuthResponse429TooManyRequestsWithInvalidRetryAfterHeaderTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<JWTAccessTokenInfo>();

            var request = ServiceRequest429ResponseConfig(1, -1);

            DateTime time1 = DateTime.UtcNow;
            DateTime time2 = time1;
            request.ExecuteAuth<JWTAccessTokenInfo>((r) =>
            {
                time2 = DateTime.UtcNow;
                response = r;
                completion.Set();
            });


            if (false == completion.WaitOne(65000))
            {
                Console.WriteLine("ExecuteAuth timeout");
                Assert.Fail();
                return;
            }

            Console.WriteLine($"request  time: {time1}");
            Console.WriteLine($"response time: {time2}");
            Assert.IsFalse(response.IsSuccess);
            Assert.IsTrue(request.m429RetryCount == 1);
            Assert.IsTrue((int)(time2 - time1).TotalSeconds == 60);
        }

        [TestMethod]
        public void ExecuteAuthResponse429TooManyRequestsWithoutRetryAfterHeaderTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<JWTAccessTokenInfo>();
            var request = ServiceRequest429ResponseConfig(1, 1, true);// no retry_after header.

            DateTime time1 = DateTime.UtcNow;
            DateTime time2 = time1;
            request.ExecuteAuth<JWTAccessTokenInfo>((r) =>
            {
                time2 = DateTime.UtcNow;
                response = r;
                completion.Set();
            });


            if (false == completion.WaitOne(65000))
            {
                Console.WriteLine("ExecuteAuth timeout");
                Assert.Fail();
                return;
            }

            Console.WriteLine($"request  time: {time1}");
            Console.WriteLine($"response time: {time2}");
            Assert.IsFalse(response.IsSuccess);
            Assert.IsTrue(request.m429RetryCount == 1);
            Assert.IsTrue((int)(time2 - time1).TotalSeconds == 60);
        }

        [TestMethod]
        public void ExecuteAuthResponse401UnAuthorizedTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<JWTAccessTokenInfo>();
            var request = ServiceRequest401ResponseConfig();

            request.ExecuteAuth<JWTAccessTokenInfo>((r) =>
            {
                response = r;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                Console.WriteLine("ExecuteAuth timeout.");
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
            Assert.IsTrue(response.Error.ErrorCode == WebexErrorCode.ServiceFailed);
            Assert.IsTrue(response.Error.Reason == "401");
            Assert.IsTrue(request.m401RetryCount == 0);
        }

        [TestMethod]
        public void ExecuteResponse429TooManyRequestsTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<JWTAccessTokenInfo>();

            var request = ServiceRequest429ResponseConfig(5, 2);

            DateTime time1 = DateTime.UtcNow;
            DateTime time2 = time1;
            request.Execute<JWTAccessTokenInfo>((r) =>
            {
                time2 = DateTime.UtcNow;
                response = r;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                Console.WriteLine("Execute timeout");
                Assert.Fail();
                return;
            }

            Console.WriteLine($"request  time: {time1}");
            Console.WriteLine($"response time: {time2}");
            Assert.IsFalse(response.IsSuccess);
            Assert.IsTrue(request.m429RetryCount == 5);
            Assert.IsTrue((int)(time2 - time1).TotalSeconds == 5 * 2);
        }

        [TestMethod]
        public void ExecuteResponse401UnAuthorizedTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<JWTAccessTokenInfo>();
            var request = ServiceRequest401ResponseConfig();

            request.Execute<JWTAccessTokenInfo>((r) =>
            {
                response = r;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                Console.WriteLine("Execute timeout.");
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
            Assert.IsTrue(response.Error.ErrorCode == WebexErrorCode.ServiceFailed);
            Assert.IsTrue(response.Error.Reason == "401");
            Assert.IsTrue(request.m401RetryCount == 2);
        }

        private ServiceRequest ServiceRequest429ResponseConfig(int max429retries, int retryAfter, bool isNoRetryAfter = false)
        {
            var foolClient = new Mock<IServiceRequestClient>();
            var foolResponse = new ServiceRequest.Response<JWTAccessTokenInfo>()
            {
                StatusCode = 429,
                StatusDescription = "TOO MANY REQUESTS"
            };
            if (isNoRetryAfter == false)
            {
                foolResponse.Headers = new List<KeyValuePair<string, object>>()
                {
                    new KeyValuePair<string, object>("Retry-After", retryAfter)
                };
            }
            foolClient.Setup(x => x.Execute<JWTAccessTokenInfo>(It.IsAny<ServiceRequest>(), It.IsAny<Action<ServiceRequest.Response<JWTAccessTokenInfo>>>()))
                .Callback<ServiceRequest, Action<ServiceRequest.Response<JWTAccessTokenInfo>>>((s, action) => action(foolResponse));

            var request = new ServiceRequest(webex.Authenticator);
            request.ClientHandler = foolClient.Object;
            ServiceRequest.MAX_429_RETRIES = max429retries;
            return request;
        }

        private ServiceRequest ServiceRequest401ResponseConfig(int? max401retries = null)
        {
            var foolClient = new Mock<IServiceRequestClient>();
            var foolResponse = new ServiceRequest.Response<JWTAccessTokenInfo>()
            {
                StatusCode = 401,
                StatusDescription = "UNAUTHORIZED",
            };
            foolClient.Setup(x => x.Execute<JWTAccessTokenInfo>(It.IsAny<ServiceRequest>(), It.IsAny<Action<ServiceRequest.Response<JWTAccessTokenInfo>>>()))
                .Callback<ServiceRequest, Action<ServiceRequest.Response<JWTAccessTokenInfo>>>((s, action) => action(foolResponse));

            var request = new ServiceRequest(webex.Authenticator);
            request.ClientHandler = foolClient.Object;
            if(max401retries != null)
            {
                ServiceRequest.MAX_401_RETRIES = (int)max401retries;
            }
            return request;
        }
    }
}
