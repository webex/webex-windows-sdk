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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Web;
using System.Threading;

namespace WebexSDK
{
    internal class SystemNetHttpClient : IServiceRequestClient
    {
        public void Execute<T>(ServiceRequest serviceRequest, Action<ServiceRequest.Response<T>> completedhandler) where T : new()
        {
            if (serviceRequest == null)
            {
                SdkLogger.Instance.Error("serviceRequest is null.");
                completedhandler?.Invoke(new ServiceRequest.Response<T>() { StatusCode = 0 });
                return;
            }

            // build request message
            var request = BuildRequestMessage(serviceRequest);

            // send async and process response
            SendAsync<T>(request, serviceRequest, completedhandler);
        }
        HttpRequestMessage BuildRequestMessage(ServiceRequest serviceRequest)
        {
            var request = new HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod(serviceRequest.Method.ToString()),
                RequestUri = new Uri(serviceRequest.BaseUri + serviceRequest.Resource),
            };
            // set authorization header
            if (serviceRequest.AccessToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceRequest.AccessToken);
            }

            // set user-agent
            if(!request.Headers.UserAgent.TryParseAdd(serviceRequest.UserAgent))
            {
                SdkLogger.Instance.Error($"add user-agent[{serviceRequest.UserAgent}] failed.");
            }

            // add customed headers
            foreach (var pair in serviceRequest.Headers)
            {
                if(!request.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
                {
                    SdkLogger.Instance.Error($"add header[{pair.Key}: {pair.Value}] failed");
                }
            }

            // add query parameters in url
            if (serviceRequest.QueryParameters != null && serviceRequest.QueryParameters.Count > 0)
            {
                var builder = new UriBuilder(request.RequestUri);
                var query = HttpUtility.ParseQueryString(builder.Query);
                foreach (var pair in serviceRequest.QueryParameters)
                {
                    query.Add(pair.Key, pair.Value);
                }
                builder.Query = query.ToString();
                request.RequestUri = builder.Uri;
            }

            // add body parameters
            if (serviceRequest.BodyParameters != null && serviceRequest.BodyParameters.Count > 0)
            {
                request.Content = new FormUrlEncodedContent(serviceRequest.BodyParameters);
            }

            return request;
        }
        void SendAsync<T>(HttpRequestMessage request, ServiceRequest serviceRequest, Action<ServiceRequest.Response<T>> completedhandler)
        {
            // Send async
            SdkLogger.Instance.Info($"http request[{request.Method}]: {request.RequestUri}");
            var client = new HttpClient();
            var task = client.SendAsync(request);

            // Process Response and Callback
            task.ContinueWith(t =>
            {
                // parse the response
                var response = t.Result;
                SdkLogger.Instance.Info($"http response[{response.StatusCode}]: {response.ReasonPhrase}");

                var outputResponse = new ServiceRequest.Response<T>
                {
                    StatusCode = (int)response.StatusCode,
                    StatusDescription = response.ReasonPhrase,
                    Headers = new List<KeyValuePair<string, object>>(),
                };

                // parse response headers
                ParseResponseHeaders<T>(response, ref outputResponse);

                // parse response content
                ParseResponseContent<T>(serviceRequest, response, ref outputResponse);

                // call back
                completedhandler.Invoke(outputResponse);
            });
        }
        void ParseResponseHeaders<T>(HttpResponseMessage response, ref ServiceRequest.Response<T> outputResponse)
        {
            if (response.Headers != null && response.Headers.Any())
            {
                foreach (var item in response.Headers.ToList())
                {
                    if (item.Value != null)
                    {
                        foreach (var v in item.Value)
                        {
                            outputResponse.Headers.Add(new KeyValuePair<string, object>(item.Key, v));
                        }
                    }
                }
            }
        }

        void ParseResponseContent<T>(ServiceRequest serviceRequest, HttpResponseMessage response, ref ServiceRequest.Response<T> outputResponse)
        {
            if (!response.IsSuccessStatusCode)
            {
                SdkLogger.Instance.Error($"http response[{response.StatusCode}]: {response.ReasonPhrase}");
                return;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return;
            }
            var result = response.Content.ReadAsStringAsync().Result;
            if (result != null && result.Length > 0)
            {
                var ser = new JavaScriptSerializer();
                if (serviceRequest.RootElement != null && serviceRequest.RootElement.Length > 0)
                {
                    try
                    {
                        dynamic parsedResult = ser.Deserialize<object>(result);
                        outputResponse.Data = ser.ConvertToType<T>(parsedResult[serviceRequest.RootElement]);
                    }
                    catch (Exception e)
                    {
                        SdkLogger.Instance.Error($"deserialize with root element exception: {e.GetType()}");
                    }
                }
                else
                {
                    try
                    {
                        outputResponse.Data = ser.Deserialize<T>(result);
                    }
                    catch (Exception e)
                    {
                        SdkLogger.Instance.Error($"deserialize without element exception: {e.GetType()}");
                    }
                }
            }
        }
    }

}
