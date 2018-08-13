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
using System.Threading;

namespace WebexSDK
{
    internal enum HttpMethod
    {
        GET = 0,
        POST = 1,
        PUT = 2,
        DELETE = 3,
        HEAD = 4,
        OPTIONS = 5,
        PATCH = 6,
        MERGE = 7
    }


    internal class ServiceRequest
    {
        public string BaseUri { get; set; }
        public HttpMethod Method { get; set; }
        private string resource;
        public string Resource
        {
            get { return resource; }
            set { resource += ("/" + value); }
        }
        public List<KeyValuePair<string, string>> Headers { get; set; }
        public List<KeyValuePair<string, object>> QueryParameters { get; set; }
        public List<KeyValuePair<string, object>> BodyParameters { get; set; }
        IAuthenticator Authenticator { get; set; }
        public string RootElement { get; set; }
        public string AccessToken { get; set; }
        public IServiceRequestClient ClientHandler { get; set; }

        // 401 options
        public static int MAX_401_RETRIES = 2;                  // Max 401 retries times

        // 429 options
        public static bool IsEligibleFor429Retry = true;        // Switch of handle 429
        public static int MAX_429_RETRIES = 0;                  // Max 429 retries times, 0 means no limit.
        public static int MAX_RETRYAFTER_SECONDS = 3600;        // Max value supported in 'retry-after' header on a 429 response
        public static int DEFAULT_RETRYAFTER_SECONDS = 60;      // Default value supported in 'retry-after' header on a 429 response

        public int m401RetryCount = 0;
        public int m429RetryCount = 0;
        private System.Timers.Timer m429RetryAfterTimer;

        public ServiceRequest()
        {
            Method = HttpMethod.GET;
            BaseUri = "https://api.ciscospark.com/v1/";
            AccessToken = null;

            Headers = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Content-Type", "application/json"),
                new KeyValuePair<string, string>("User-Agent", UserAgent.Instance.Name),
                new KeyValuePair<string, string>("Webex-User-Agent", UserAgent.Instance.Name)
            };
            QueryParameters = new List<KeyValuePair<string, object>>();
            BodyParameters = new List<KeyValuePair<string, object>>();
            Resource = "";
            RootElement = "";

            ClientHandler = new RestSharpClient();
        }

        public ServiceRequest(IAuthenticator authenticator)
            : this()
        {
            this.Authenticator = authenticator;
        }

        public ServiceRequest(IAuthenticator authenticator, HttpMethod method)
            : this()
        {
            this.Authenticator = authenticator;
            this.Method = method;
        }

        public void AddQueryParameters(string name, object value)
        {
            QueryParameters.Add(new KeyValuePair<string, object>(name, value));
        }

        public void AddBodyParameters(string name, object value)
        {
            BodyParameters.Add(new KeyValuePair<string, object>(name, value));
        }

        public void AddHeaders(string name, string value)
        {
            Headers.Add(new KeyValuePair<string, string>(name, value));
        }


        public void Execute<T>(Action<WebexApiEventArgs<T>> completedhandler) where T : new()
        {
            Authenticator?.AccessToken(response =>
            {
                if (response == null || !response.IsSuccess || response.Data == null)
                {
                    SdkLogger.Instance.Error("ServiceRequest.Execute.accessToken Failed");
                    completedhandler?.Invoke(new WebexApiEventArgs<T>(false, null, default(T)));
                    return;
                }

                this.AccessToken = (string)response.Data;

                ClientHandler.Execute<T>(this, resp =>
                {
                    HandleResponse<T>(resp, completedhandler);
                });

            });
        }

        public void ExecuteAuth<T>(Action<WebexApiEventArgs<T>> completedhandler) where T : new()
        {
            ClientHandler.Execute<T>(this, resp =>
            {
                HandleAuthResponse<T>(resp, completedhandler);
            });
        }

        private void HandleAuthResponse<T>(ServiceRequest.Response<T> resp, Action<WebexApiEventArgs<T>> completedhandler) where T : new()
        {
            SdkLogger.Instance.Debug($"http response: {resp.StatusCode}");
            if (resp.StatusCode >= 200 && resp.StatusCode < 300)
            {
                SdkLogger.Instance.Info("http response success");
                completedhandler?.Invoke(new WebexApiEventArgs<T>(true, null, resp.Data));
            }
            else if (429 == resp.StatusCode && IsEligibleFor429Retry)
            {
                HandleResponse429TooManyRequests(true, resp, completedhandler);
            }
            else
            {
                SdkLogger.Instance.Error($"http response error: {resp.StatusCode}");
                completedhandler?.Invoke(new WebexApiEventArgs<T>(false, new WebexError(WebexErrorCode.ServiceFailed, resp.StatusCode.ToString()), default(T)));
            }
        }

        private void HandleResponse<T>(ServiceRequest.Response<T> resp, Action<WebexApiEventArgs<T>> completedhandler) where T : new()
        {
            SdkLogger.Instance.Debug($"http response: {resp.StatusCode}");

            if (resp.StatusCode >= 200 && resp.StatusCode < 300)
            {
                SdkLogger.Instance.Info("http response success");
                completedhandler?.Invoke(new WebexApiEventArgs<T>(true, null, resp.Data));
            }
            else if (429 == resp.StatusCode && IsEligibleFor429Retry)
            {
                HandleResponse429TooManyRequests(false, resp, completedhandler);
            }
            else if (401 == resp.StatusCode)
            {
                HandleResponse401UnAuthorized(resp, completedhandler);
            }
            else
            {
                SdkLogger.Instance.Error($"http response error: {resp.StatusCode} {resp.StatusDescription}");
                completedhandler?.Invoke(new WebexApiEventArgs<T>(false, new WebexError(WebexErrorCode.ServiceFailed, resp.StatusCode.ToString()), default(T)));
            }
        }

        private int GetRetryAfterValue<T>(ServiceRequest.Response<T> resp) where T : new()
        {
            int retryAfter;
            try
            {
                var r = resp.Headers.Find(x => x.Key == "Retry-After");
                SdkLogger.Instance.Debug($"RCV 429, retry_after value: {(int)r.Value} seconds.");

                retryAfter = (int)r.Value > MAX_RETRYAFTER_SECONDS ? MAX_RETRYAFTER_SECONDS : (int)r.Value;
                retryAfter = retryAfter <= 0 ? DEFAULT_RETRYAFTER_SECONDS : retryAfter;
            }
            catch
            {
                SdkLogger.Instance.Debug($"In 429 response, there is no Retry-After header. Set default value[{DEFAULT_RETRYAFTER_SECONDS}] seconds.");
                retryAfter = DEFAULT_RETRYAFTER_SECONDS;
            }

            return retryAfter;
        }

        private void HandleResponse429TooManyRequests<T>(bool isAuthProcess,ServiceRequest.Response<T> resp, Action<WebexApiEventArgs<T>> completedhandler) where T : new()
        {
            if (MAX_429_RETRIES != 0 && m429RetryCount >= MAX_429_RETRIES)
            {
                SdkLogger.Instance.Warn($"429 retry exceed MAX_429_RETRIES[{MAX_429_RETRIES}] times.");
                completedhandler?.Invoke(new WebexApiEventArgs<T>(false, new WebexError(WebexErrorCode.ServiceFailed, resp.StatusCode.ToString()), default(T)));
                return;
            }

            int retryAfter = GetRetryAfterValue(resp);

            // start timer after retryAfter seconds and retry request
            SdkLogger.Instance.Debug($"start timer: {retryAfter} seconds.");
            m429RetryAfterTimer = TimerHelper.StartTimer(retryAfter * 1000, (o, e) =>
            {
                m429RetryCount++;
                SdkLogger.Instance.Debug("429 retry began.");
                if(isAuthProcess)
                {
                    ExecuteAuth<T>(completedhandler);
                }
                else
                {
                    Execute<T>(completedhandler);
                }
                
            });
        }
        private void HandleResponse401UnAuthorized<T>(ServiceRequest.Response<T> resp, Action<WebexApiEventArgs<T>> completedhandler) where T : new()
        {
            if (m401RetryCount >= MAX_401_RETRIES)
            {
                SdkLogger.Instance.Warn($"401 refresh retry exceed MAX_401_RETRIES[{MAX_401_RETRIES}] times.");
                completedhandler?.Invoke(new WebexApiEventArgs<T>(false, new WebexError(WebexErrorCode.ServiceFailed, resp.StatusCode.ToString()), default(T)));
                return;
            }
            m401RetryCount++;

            Authenticator?.RefreshToken(r =>
            {
                if (r.IsSuccess)
                {
                    SdkLogger.Instance.Debug("401 refresh token success and began to retry.");
                    Execute<T>(completedhandler);
                }
                else
                {
                    SdkLogger.Instance.Error("401 refresh token fail.");
                    completedhandler?.Invoke(new WebexApiEventArgs<T>(false, new WebexError(WebexErrorCode.ServiceFailed, resp.StatusCode.ToString()), default(T)));
                }
            });
        }

        public class Response<T>
        {
            // HTTP response status code
            public int StatusCode { get; set; }
            
            // Description of HTTP status returned
            public string StatusDescription { get; set; }

            // Headers returned by server with the response
            public List<KeyValuePair<string, object>> Headers { get; set; }

            // Deserialized entity data
            public T Data { get; set; }
        }


    }

}
