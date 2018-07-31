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


namespace WebexSDK
{
    /// <summary>
    /// WebhookClient allow your app to be notified via HTTP when a specific event occurs on Webex. For example, your app can register a webhook to be notified when a new message is posted into a specific space
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class WebhookClient
    {
        readonly IAuthenticator authenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookClient"/> class.
        /// </summary>
        /// <param name="authenticator">The authenticator.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public WebhookClient(IAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        private ServiceRequest BuildRequest()
        {
            var request = new ServiceRequest(authenticator)
            {
                Resource = "webhooks",
            };
            return request;
        }

        /// <summary>
        /// Lists all webhooks of the authenticated user.
        /// </summary>
        /// <param name="max">The maximum number of webhooks in the response.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void List(int? max, Action<WebexApiEventArgs<List<Webhook>>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.RootElement = "items";
            if (max != null) request.AddQueryParameters("max", max);

            request.Execute<List<Webhook>>(completionHandler);
        }

        /// <summary>
        /// Posts a webhook for the authenticated user.
        /// </summary>
        /// <param name="name">A user-friendly name for this webhook.</param>
        /// <param name="targetUrl">The URL that receives POST requests for each event.</param>
        /// <param name="resource">The resource type for the webhook.</param>
        /// <param name="eventType">The event type for the webhook.</param>
        /// <param name="filter">The filter that defines the webhook scope.</param>
        /// <param name="secret">Secret use to generate payload signiture</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Create(string name, string targetUrl, string resource, string eventType, string filter, string secret, Action<WebexApiEventArgs<Webhook>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.POST;
            if (name != null)           request.AddBodyParameters("name", name);
            if (targetUrl != null)      request.AddBodyParameters("targetUrl", targetUrl);
            if (resource != null)       request.AddBodyParameters("resource", resource);
            if (eventType != null)      request.AddBodyParameters("event", eventType);
            if (filter != null)         request.AddBodyParameters("filter", filter);
            if (secret != null)         request.AddBodyParameters("secret", secret);

            request.Execute<Webhook>(completionHandler);
        }

        /// <summary>
        /// Retrieves the details for a webhook by id.
        /// </summary>
        /// <param name="webhookId">The identifier of  the webhook.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Get(string webhookId, Action<WebexApiEventArgs<Webhook>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.Resource = webhookId;

            request.Execute<Webhook>(completionHandler);
        }

        /// <summary>
        /// Updates a webhook by id.
        /// </summary>
        /// <param name="webhookId">The identifier of  the webhook.</param>
        /// <param name="name">A user-friendly name for this webhook.</param>
        /// <param name="targetUrl">The URL that receives POST requests for each event.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Update(string webhookId, string name, string targetUrl, Action<WebexApiEventArgs<Webhook>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.PUT;
            request.Resource = webhookId;
            if (name != null) request.AddQueryParameters("name", name);
            if (targetUrl != null) request.AddQueryParameters("targetUrl", targetUrl);

            request.Execute<Webhook>(completionHandler);
        }

        /// <summary>
        /// Deletes a webhook by id.
        /// </summary>
        /// <param name="webhookId">The identifier of  the webhook.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Delete(string webhookId, Action<WebexApiEventArgs> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.DELETE;
            request.Resource = webhookId;

            request.Execute<bool>(completionHandler);
        }
    }
}
