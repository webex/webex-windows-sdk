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
    /// Class TeamClient contains APIs which are used to manage the teams themselves. 
    /// Teams are created and deleted with this API. You can also update a team to change its title 
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class TeamClient
    {
        readonly IAuthenticator authenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamClient"/> class.
        /// </summary>
        /// <param name="authenticator">The authenticator.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public TeamClient(IAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        private ServiceRequest BuildRequest()
        {
            var request = new ServiceRequest(authenticator)
            {
                Resource = "teams",
            };
            return request;
        }

        /// <summary>
        /// Lists teams to which the authenticated user belongs.
        /// </summary>
        /// <param name="max">The maximum number of teams in the response. if null, list all teams.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void List(int? max, Action<WebexApiEventArgs<List<Team>>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.RootElement = "items";
            if (max != null) request.AddQueryParameters("max", max);

            request.Execute<List<Team>>(completionHandler);
        }

        /// <summary>
        /// Creates a team. The authenticated user is automatically added as a member of the team. 
        /// See the Team Memberships API to learn how to add more people to the team.
        /// </summary>
        /// <param name="name">A user-friendly name for the team.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Create(string name, Action<WebexApiEventArgs<Team>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.POST;
            if (name != null) request.AddBodyParameters("name", name);

            request.Execute<Team>(completionHandler);
        }


        /// <summary>
        /// Retrieves the details for a team by id.
        /// </summary>
        /// <param name="teamId">The identifier of the team.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Get(string teamId, Action<WebexApiEventArgs<Team>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.Resource = teamId;

            request.Execute<Team>(completionHandler);
        }

        /// <summary>
        /// Updates the details for a team by id.
        /// </summary>
        /// <param name="teamId">The team id.</param>
        /// <param name="name">A user-friendly name for the team.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Update(string teamId, string name, Action<WebexApiEventArgs<Team>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.PUT;
            request.Resource = teamId;
            if (name != null) request.AddQueryParameters("name", name);

            request.Execute<Team>(completionHandler);
        }

        /// <summary>
        /// Deletes a team by id.
        /// </summary>
        /// <param name="teamId">The team id.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Delete(string teamId, Action<WebexApiEventArgs> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.DELETE;
            request.Resource = teamId;

            request.Execute<bool>(completionHandler);
        }
    }
}
