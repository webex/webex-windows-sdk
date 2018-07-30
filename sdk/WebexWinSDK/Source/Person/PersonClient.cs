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
    /// Person are registered users of Cisco Webex.
    /// Searching and viewing Person requires an auth token with a scope of webex:people_read. 
    /// Viewing the list of all People in your Organization requires an administrator auth token with webex-admin:people_read scope. 
    /// Adding, updating, and removing People requires an administrator auth token with the webex-admin:people_write scope.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class PersonClient
    {
        readonly IAuthenticator authenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonClient"/> class.
        /// </summary>
        /// <param name="authenticator">The authenticator.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public PersonClient(IAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        private ServiceRequest BuildRequest()
        {
            var request = new ServiceRequest(authenticator, HttpMethod.GET)
            {
                Resource = "people",
            };
            return request;
        }

        /// <summary>
        /// Lists people in the authenticated user's organization.
        /// </summary>
        /// <param name="email">if not null, only list people with this email address.</param>
        /// <param name="displayName">if not null, only list people whose name starts with this string.</param>
        /// <param name="max">The maximum number of people in the response.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void List(string email, string displayName, int? max, Action<WebexApiEventArgs<List<Person>>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.RootElement = "items";
            if(email != null)       request.AddQueryParameters("email", email);
            if(displayName != null) request.AddQueryParameters("displayName", displayName);
            if(max != null)         request.AddQueryParameters("max", max);        

            request.Execute<List<Person>>(completionHandler);
        }


        /// <summary>
        /// Retrieves the details for a person by person id.
        /// </summary>
        /// <param name="personId">The identifier of the person.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Get(string personId, Action<WebexApiEventArgs<Person>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Resource = personId;

            request.Execute<Person>(completionHandler);
        }

        /// <summary>
        /// Retrieves the details for the authenticated user.
        /// </summary>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void GetMe(Action<WebexApiEventArgs<Person>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Resource = "me";

            request.Execute<Person>(completionHandler);
        }
    }
}
