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
    /// Memberships represent a person's relationship to a room. 
    /// Use this API to list members of any room that you're in or create memberships to invite someone to a room. 
    /// Memberships can also be updated to make someome a moderator or deleted to remove them from the room.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class MembershipClient
    {
        readonly IAuthenticator authenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="MembershipClient"/> class.
        /// </summary>
        /// <param name="authenticator">The authenticator.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public MembershipClient(IAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        private ServiceRequest BuildRequest()
        {
            var request = new ServiceRequest(authenticator)
            {
                Resource = "memberships",
            };
            return request;
        }

        /// <summary>
        /// Lists all room memberships where the authenticated user belongs.
        /// </summary>
        /// <param name="max">The maximum number of items in the response.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void List(int? max, Action<WebexApiEventArgs<List<Membership>>> completionHandler)
        {
            List(null,null,null,max, completionHandler);
        }

        /// <summary>
        /// Lists all memberships in the given room by room Id.
        /// </summary>
        /// <param name="roomId">The identifier of the room where the membership belongs.</param>
        /// <param name="max">The maximum number of memberships in the response.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void List(string roomId, int? max, Action<WebexApiEventArgs<List<Membership>>> completionHandler)
        {
            List(roomId, null, null, max, completionHandler);
        }

        /// <summary>
        /// Lists any room memberships for the given room (by room id) and person (by person id).
        /// </summary>
        /// <param name="roomId">The identifier of the room where the memberships belong.</param>
        /// <param name="personId">The identifier of the person who has the memberships.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void ListByPersonId(string roomId, string personId, Action<WebexApiEventArgs<List<Membership>>> completionHandler)
        {
            List(roomId, personId, null, null, completionHandler);
        }

        /// <summary>
        /// Lists any room memberships for the given room (by room id) and person (by email address).
        /// </summary>
        /// <param name="roomId">The identifier of the room where the memberships belong.</param>
        /// <param name="personEmail">The email address of the person who has the memberships.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void ListByPersonEmail(string roomId, string personEmail, Action<WebexApiEventArgs<List<Membership>>> completionHandler)
        {
            List(roomId, null, personEmail, null, completionHandler);

        }

        private void List(string roomId, string personId, string personEmail, int? max, Action<WebexApiEventArgs<List<Membership>>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.RootElement = "items";
            if (roomId != null)         request.AddQueryParameters("roomId", roomId);
            if (personId != null)       request.AddQueryParameters("personId", personId);
            if (personEmail != null)    request.AddQueryParameters("personEmail", personEmail);
            if (max != null)            request.AddQueryParameters("max", max);

            request.Execute<List<Membership>>(completionHandler);
        }


        /// <summary>
        /// Adds a person to a room by person id; optionally making the person a moderator.
        /// </summary>
        /// <param name="roomId">The identifier of the room where the person is to be added.</param>
        /// <param name="personId">The identifier of the person to be added.</param>
        /// <param name="isModerator">if set to <c>true</c> [is moderator of the room]. The default is false.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void CreateByPersonId(string roomId, string personId, bool? isModerator = false, Action<WebexApiEventArgs<Membership>> completionHandler = null)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.POST;
            if (roomId != null) request.AddBodyParameters("roomId", roomId);
            if (personId != null) request.AddBodyParameters("personId", personId);
            if (isModerator != null) request.AddBodyParameters("isModerator", isModerator);

            request.Execute<Membership>(completionHandler);
        }

        /// <summary>
        /// Adds a person to a room by email address; optionally making the person a moderator.
        /// </summary>
        /// <param name="roomId">The identifier of the room where the person is to be added.</param>
        /// <param name="personEmail">The email address of the person to be added.</param>
        /// <param name="isModerator">if set to <c>true</c> [is moderator of the room]. The default is false.</param>
        /// <param name="completionHandler">The completion handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void CreateByPersonEmail(string roomId, string personEmail, bool? isModerator = false, Action<WebexApiEventArgs<Membership>> completionHandler = null)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.POST;
            if (roomId != null) request.AddBodyParameters("roomId", roomId);
            if (personEmail != null) request.AddBodyParameters("personEmail", personEmail);
            if (isModerator != null) request.AddBodyParameters("isModerator", isModerator);

            request.Execute<Membership>(completionHandler);
        }

        /// <summary>
        /// Retrieves the details for a membership by membership id.
        /// </summary>
        /// <param name="membershipId">The identifier of the membership.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Get(string membershipId, Action<WebexApiEventArgs<Membership>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.Resource = membershipId;

            request.Execute<Membership>(completionHandler);
        }

        /// <summary>
        /// Updates the properties of a membership by membership id.
        /// </summary>
        /// <param name="membershipId">The identifier of the membership.</param>
        /// <param name="isModerator">if set to <c>true</c> [is moderator of the room]. The default is false.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Update(string membershipId, bool? isModerator, Action<WebexApiEventArgs<Membership>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.PUT;
            request.Resource = membershipId;
            if (isModerator != null) request.AddQueryParameters("isModerator", isModerator);

            request.Execute<Membership>(completionHandler);
        }

        /// <summary>
        /// Deletes a membership by membership id. It removes the person from the room where the membership belongs.
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
