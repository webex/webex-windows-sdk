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
    /// A data type presents a Webhook at Cisco Webex for Developer.
    /// </summary>
    /// <remarks>
    /// see [Webhook Explained](https://developer.webex.com/webhooks-explained.html)
    /// </remarks>
    /// <remarks>Since: 0.1.0</remarks>
    public class Webhook
    {

        /// <summary>
        /// The identifier of this webhook.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Id { get; set; }

        /// <summary>
        /// A user-friendly name for this webhook.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Name { get; set; }

        /// <summary>
        /// The URL that receives POST requests for each event.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string TargetUrl { get; set; }

        /// <summary>
        /// The resource type for the webhook.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Resource { get; set; }

        /// <summary>
        /// The event type for the webhook.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Event { get; set; }

        /// <summary>
        /// The filter that defines the webhook scope.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Filter { get; set; }

        /// <summary>
        /// The timestamp that the webhook being created.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public DateTime Created { get; set; }

        /// <summary>
        /// The status of the webhook. Use <code>active</code> to reactivate a disabled webhook.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The Secret use to generate payload signature.
        /// </summary>
        public string Secret { get; set; }

    }
}
