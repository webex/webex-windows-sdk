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
    /// Class SpaceClient contains APIs which are used to manage the spaces themselves. 
    /// Spaces are created and deleted with this API. You can also update a space to change its title 
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class SpaceClient
    {
        readonly IAuthenticator authenticator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpaceClient"/> class.
        /// </summary>
        /// <param name="authenticator">The authenticator.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public SpaceClient(IAuthenticator authenticator )
        {
            this.authenticator = authenticator;
        }

        private ServiceRequest BuildRequest()
        {
            var request = new ServiceRequest(authenticator)
            {
                Resource = "rooms",
            };
            return request;
        }

        /// <summary>
        /// Lists all spaces where the authenticated user belongs.
        /// </summary>
        /// <param name="teamId">If not null, only list the spaces that are associated with the team by team id.</param>
        /// <param name="max">The maximum number of spaces in the response. If null, all spaces are listed.</param>
        /// <param name="type">If not null, only list the spaces of this type. Otherwise all spaces are listed.</param>
        /// <param name="sortBy">If not null, sort results by spaceId(id), most recent activity(lastactivity), or most recently created(created).</param>
        /// <param name="completionHandler">The completion handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void List(string teamId, int? max, SpaceType? type, SpaceSortType? sortBy, Action<WebexApiEventArgs<List<Space>>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.RootElement = "items";
            if (teamId != null) request.AddQueryParameters("teamId", teamId);
            if (max != null) request.AddQueryParameters("max", max);
            if (type != null) request.AddQueryParameters("type", type.ToString().ToLower());
            if (sortBy != null)
            {
                string strSortBy = null;
                switch (sortBy)
                {
                    case SpaceSortType.ById:
                        strSortBy = "id";
                        break;
                    case SpaceSortType.ByLastActivity:
                        strSortBy = "lastactivity";
                        break;
                    case SpaceSortType.ByCreated:
                        strSortBy = "created";
                        break;
                    default:
                        completionHandler?.Invoke(new WebexApiEventArgs<List<Space>>(false, new WebexError(WebexErrorCode.IllegalOperation, "sort type is invalid."), null));
                        return;
                }
                request.AddQueryParameters("sortBy", strSortBy);
            }

            request.Execute<List<Space>>(completionHandler);

        }

        /// <summary>
        /// Creates a space. The authenticated user is automatically added as a member of the space. See the Memberships API to learn how to add more people to the space.
        /// </summary>
        /// <param name="title">A user-friendly name for the space.</param>
        /// <param name="teamId">If not null, this space will be associated with the team by team id. Otherwise, this space is not associated with any team.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Create( string title, string teamId, Action<WebexApiEventArgs<Space>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.POST;
            if (title != null) request.AddBodyParameters("title", title);
            if (teamId != null) request.AddBodyParameters("teamId", teamId);

            request.Execute<Space>(completionHandler);
        }

        /// <summary>
        /// Retrieves the details for a space by id.
        /// </summary>
        /// <param name="spaceId">The identifier of the space.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Get(string spaceId, Action<WebexApiEventArgs<Space>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.GET;
            request.Resource = spaceId;

            request.Execute<Space>(completionHandler);
        }

        /// <summary>
        /// Updates the details for a space by id.
        /// </summary>
        /// <param name="spaceId">The identifier of the space.</param>
        /// <param name="title">A user-friendly name for the space.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Update(string spaceId, string title, Action<WebexApiEventArgs<Space>> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.PUT;
            request.Resource = spaceId;
            if (title != null) request.AddQueryParameters("title", title);

            request.Execute<Space>(completionHandler);
        }

        /// <summary>
        /// Deletes a space by id.
        /// </summary>
        /// <param name="spaceId">The identifier of the space.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Delete(string spaceId, Action<WebexApiEventArgs> completionHandler)
        {
            ServiceRequest request = BuildRequest();
            request.Method = HttpMethod.DELETE;
            request.Resource = spaceId;

            request.Execute<bool>(completionHandler);
        }

    }
}
