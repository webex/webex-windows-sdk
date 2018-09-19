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
    /// TeamMemberships represent a person's relationship to a team. 
    /// Use this API to list members of any team that you're in or create memberships to invite someone to a team. 
    /// Team memberships can also be updated to make someone a moderator or deleted to remove them from the team. 
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class TeamMembershipClient
    {
        readonly IAuthenticator authenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamMembershipClient"/> class.
        /// </summary>
        /// <param name="authenticator">The authenticator.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public TeamMembershipClient(IAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        private ServiceRequest BuildRequest()
        {
            var request = new ServiceRequest(authenticator)
            {
                Resource = "team/memberships",
            };
            return request;
        }

        /// <summary>
        /// Lists all team memberships where the authenticated user belongs.
        /// </summary>
        /// <param name="teamId">Limit results to a specific team, by ID.</param>
        /// <param name="max">The maximum number of team memberships in the response.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void List(string teamId, int? max, Action<WebexApiEventArgs<List<TeamMembership>>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.RootElement = "items";
            if (teamId != null) request.AddQueryParameters("teamId", teamId);
            if (max != null) request.AddQueryParameters("max", max.ToString());

            request.Execute<List<TeamMembership>>(completionHandler);
        }


        /// <summary>
        /// Adds a person to a team by person id; optionally making the person a moderator of the team.
        /// </summary>
        /// <param name="teamId">The identifier of the team.</param>
        /// <param name="personId">The identifier of the person.</param>
        /// <param name="isModerator">if set to <c>true</c> [is moderator of the team]. The default is false.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void CreateById(string teamId, string personId, bool? isModerator = false, Action<WebexApiEventArgs<TeamMembership>> completionHandler = null)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.POST;
            if (teamId != null)         request.AddBodyParameters("teamId", teamId);
            if (personId != null)       request.AddBodyParameters("personId", personId);
            if (isModerator != null)    request.AddBodyParameters("isModerator", isModerator.ToString());

            request.Execute<TeamMembership>(completionHandler);
        }

        /// <summary>
        /// Add a person to a teams by email address; optionally making the person a moderator of the team.
        /// </summary>
        /// <param name="teamId">The identifier of the team.</param>
        /// <param name="personEmail">The email address of the person.</param>
        /// <param name="isModerator">if set to <c>true</c> [is moderator of the team]. The default is false.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void CreateByEmail(string teamId, string personEmail, bool? isModerator = false, Action<WebexApiEventArgs<TeamMembership>> completionHandler = null)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.POST;
            if (teamId != null) request.AddBodyParameters("teamId", teamId);
            if (personEmail != null) request.AddBodyParameters("personEmail", personEmail);
            if (isModerator != null) request.AddBodyParameters("isModerator", isModerator.ToString());

            request.Execute<TeamMembership>(completionHandler);
        }


        /// <summary>
        /// Retrieves the details for a membership by id.
        /// </summary>
        /// <param name="membershipId">The identifier of the membership.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Get(string membershipId, Action<WebexApiEventArgs<TeamMembership>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.Resource = membershipId;

            request.Execute<TeamMembership>(completionHandler);
        }

        /// <summary>
        /// Updates the details for a membership by id.
        /// </summary>
        /// <param name="membershipId">The identifier of the membership.</param>
        /// <param name="isModerator">if set to <c>true</c> [is moderator of the team]. The default is false.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Update(string membershipId, bool? isModerator, Action<WebexApiEventArgs<TeamMembership>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.PUT;
            request.Resource = membershipId;
            if (isModerator != null) request.AddBodyParameters("isModerator", isModerator.ToString());

            request.Execute<TeamMembership>(completionHandler);
        }

        /// <summary>
        /// Deletes a membership by id.
        /// </summary>
        /// <param name="membershipId">The identifier of the membership.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Delete(string membershipId, Action<WebexApiEventArgs> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.DELETE;
            request.Resource = membershipId;

            request.Execute<bool>(completionHandler);
        }
    }
}
