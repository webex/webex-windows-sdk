using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebexSDK;
#region License
// Copyright (c) 2016-2018 Cisco Systems, Inc.

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;

namespace WebexSDK.Tests
{
    [TestClass()]
    public class JWTAuthenticatorTests
    {
        string jwt = ConfigurationManager.AppSettings["JWT"] ?? "";
        [TestInitialize]
        public void SetUp()
        {
        }


        [TestCleanup]
        public void TearDown()
        {
            Thread.Sleep(10000);
        }
        [TestMethod()]
        public void AuthorizeWithTest()
        {
            var auth = new JWTAuthenticator();

            Assert.IsTrue(LoginWithTryTimes(auth, 5));
            Assert.IsTrue(Logout(auth));   
        }

        [TestMethod()]
        public void AuthorizeTestInvalidJWTnull()
        {
            var completion = new ManualResetEvent(false);

            var auth = new JWTAuthenticator();
            var response = new WebexApiEventArgs<string>();
            auth.AccessToken(r =>
            {
                response = r;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }
            Assert.IsFalse(response.IsSuccess);
        }

        [TestMethod()]
        public void AuthorizeTestInvalidJWT()
        {
            var completion = new ManualResetEvent(false);

            var auth = new JWTAuthenticator();
            string jwt = "a.c";

            var rspOfauth = new WebexApiEventArgs();
            auth.AuthorizeWith(jwt, r =>
            {
                rspOfauth = r;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }
            Assert.IsFalse(rspOfauth.IsSuccess);
        }

        [TestMethod()]
        public void AuthorizeTestInvalidJWTexpire()
        {
            var completion = new ManualResetEvent(false);

            var auth = new JWTAuthenticator();
            //expired JWT
            string jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG5Eb2UiLCJleHAiOiIxNDA1OTc0ODczIiwiaXNzIjoiY2Q1YzlhZjctOGVkMy00ZTE1LTk3MDUtMDI1ZWYzMGIxYjZhIn0.brzUE0LYgEIkt4kK7s2QwrHkhgWPUwbj5XMVECAA_hQ";

            var rspOfauth = new WebexApiEventArgs();
            auth.AuthorizeWith(jwt, r =>
            {
                rspOfauth = r;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }
            Assert.IsFalse(rspOfauth.IsSuccess);
        }

        [TestMethod()]
        public void AuthorizeTestErrorJwt1()
        {
            var completion = new ManualResetEvent(false);

            var auth = new JWTAuthenticator();

            var rspOfauth = new WebexApiEventArgs();
            auth.AuthorizeWith("a.b.c", r =>
            {
                rspOfauth = r;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }
            Assert.IsFalse(rspOfauth.IsSuccess);
        }

        [TestMethod()]
        public void AuthorizeTestErrorJwt2()
        {
            var completion = new ManualResetEvent(false);

            var auth = new JWTAuthenticator();
            string jwt = "yJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG5Eb2UiLCJpc3MiOiJjZDVjOWFmNy04ZWQzLTRlMTUtOTcwNS0wMjVlZjMwYjFiNmEifQ.oC-QPs-Eotaq4ovv2glwrHpxXQzqN1WvNlKmMAmtp24";
            var rspOfauth = new WebexApiEventArgs();
            auth.AuthorizeWith(jwt, r=>
            {
                rspOfauth = r;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }
            Assert.IsFalse(rspOfauth.IsSuccess);
        }


        [TestMethod()]
        public void AccessTokenTest()
        {
            var completion = new ManualResetEvent(false);
            var repOfaccessToken = new WebexApiEventArgs<string>();

            var auth = new JWTAuthenticator();
            Assert.IsTrue(LoginWithTryTimes(auth, 5));
            
            auth.AccessToken(r =>
            {
                repOfaccessToken = r;
                completion.Set();
            });
            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(repOfaccessToken.IsSuccess);
            Assert.IsNotNull(repOfaccessToken.Data);
            Assert.IsNull(repOfaccessToken.Error);

            Assert.IsTrue(Logout(auth));

            Thread.Sleep(30000);
        }

        //[TestMethod()]
        //public void AccessTokenTestUnexpirAccessToken()
        //{
        //    var completion = new ManualResetEvent(false);
        //    var repOfaccessToken = new WebexApiEventArgs<string>();

        //    var auth = new JWTAuthenticator();
        //    Assert.IsTrue(LoginWithTryTimes(auth, 5));

        //    auth.AccessToken(r =>
        //    {
        //        repOfaccessToken = r;
        //        completion.Set();
        //    });
        //    if (false == completion.WaitOne(30000))
        //    {
        //        Assert.Fail();
        //        return;
        //    }

        //    Assert.IsTrue(repOfaccessToken.IsSuccess);
        //    Assert.IsNotNull(repOfaccessToken.Data);
        //    Assert.IsNull(repOfaccessToken.Error);


        //    //get unexpired access token, it is the same of last one.
        //    completion.Reset();

        //    var newAccessToken = new WebexApiEventArgs<string>();
        //    auth.AccessToken(r =>
        //    {
        //        newAccessToken = r;
        //        completion.Set();
        //    });
        //    if (false == completion.WaitOne(30000))
        //    {
        //        Assert.Fail();
        //        return;
        //    }

        //    Assert.IsTrue(newAccessToken.IsSuccess);
        //    Assert.AreEqual(repOfaccessToken.Data, newAccessToken.Data);

        //    Assert.IsTrue(Logout(auth));
        //    Thread.Sleep(30000);
        //}

        //[TestMethod()]
        //public void LoginLogoutManytimes()
        //{
        //    var auth = new JWTAuthenticator();
        //    int count = 3;
        //    while (count > 0)
        //    {
        //        count--;
        //        Console.WriteLine($"loginLogoutManytimes[{count}]");
        //        Assert.IsTrue(loginWithTryTimes(auth, 10));
        //        logout(auth);
        //    }
        //}

        private bool CheckAuthorized(JWTAuthenticator auth)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
            auth.Authorized(r =>
            {
                response = r;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return false;
            }
            return response.IsSuccess;
        }

        private bool Login(JWTAuthenticator auth)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();

            if (auth == null)
            {
                Console.WriteLine("jwt login: auth is null");
                return false;
            }
            auth.AuthorizeWith(jwt, r =>
            {
                response = r;
                completion.Set();
            });
            if (false == completion.WaitOne(30000))
            {
                Console.WriteLine("jwt login: timeout");
                return false ;
            }

            Console.WriteLine($"jwt login: success is {response.IsSuccess}");
            return response.IsSuccess;
        }

        private bool LoginWithTryTimes(JWTAuthenticator auth, int times)
        {
            int tryTimes = times;
            while (tryTimes > 0)
            {
                tryTimes--;
                if (Login(auth))
                {
                    Console.WriteLine("loginWithTryTimes: success");
                    return true;
                }
            }
            Console.WriteLine("loginWithTryTimes: fail");
            return false;      
        }

        private bool Logout(JWTAuthenticator auth)
        {
            Thread.Sleep(5000);
            if (auth == null)
            {
                return false;
            }
            auth.Deauthorize();
            Console.WriteLine("jwt logout: success");
            return true;
        }
    }
}