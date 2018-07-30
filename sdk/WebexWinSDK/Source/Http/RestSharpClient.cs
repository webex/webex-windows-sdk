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
using System.Net;
using System.Text;
using RestSharp;

namespace WebexSDK
{
    internal class RestSharpClient : IServiceRequestClient
    {
        public void Execute<T>(ServiceRequest serviceRequest, Action<ServiceRequest.Response<T>> completedhandler) where T : new()
        {
            if (serviceRequest == null)
            {
                SDKLogger.Instance.Error("serviceRequest is null.");
                completedhandler?.Invoke(new ServiceRequest.Response<T>() { StatusCode = 0});
                return;
            }

            RestRequest request = new RestRequest(serviceRequest.Resource, (Method)serviceRequest.Method);

            if (serviceRequest.AccessToken != null)
            {
                request.AddHeader("Authorization", "Bearer " + serviceRequest.AccessToken);
            }

            foreach (var pair in serviceRequest.Headers)
            {
                request.AddHeader(pair.Key, pair.Value);
            }

            foreach (var pair in serviceRequest.QueryParameters)
            {
                request.AddParameter(pair.Key, pair.Value, ParameterType.GetOrPost);
            }

            foreach (var pair in serviceRequest.BodyParameters)
            {
                request.AddParameter(pair.Key, pair.Value, ParameterType.GetOrPost);
            }

            if (serviceRequest.RootElement.Length != 0)
            {
                request.RootElement = serviceRequest.RootElement;
            }
            //Cisco Webex platform is dropping support for TLS 1.0 as of March 16, 2018
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            var client = new RestClient()
            {
                BaseUrl = new System.Uri(serviceRequest.BaseUri)
            };
            

            SDKLogger.Instance.Info($"http request[{serviceRequest.Method.ToString()}]: {serviceRequest.BaseUri + request.Resource}" );
            client.ExecuteAsync<T>(request, response =>
            {
                var r = new ServiceRequest.Response<T>();
                r.StatusCode = (int)response.StatusCode;
                r.StatusDescription = response.StatusDescription;
                r.Headers = new List<KeyValuePair<string, object>>();
                foreach (var i in response.Headers)
                {
                    r.Headers.Add(new KeyValuePair<string, object>(i.Name, i.Value));
                }

                r.Data = response.Data;

                completedhandler?.Invoke(r);
            });
        }
    }
}
