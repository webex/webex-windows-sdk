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

namespace WebexSDK
{
    /// <summary>
    /// An Single sign-on [SSO](https://help.webex.com/docs/DOC-9143#reference_E9B2CEDE975E4CD311C56D9B0EF2476C)
    /// based authentication strategy used to authenticate a user on Cisco Webex.
    /// See: [Cisco Webex Integration](https://developer.webex.com/authentication.html)
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class SSOAuthenticator : OAuthAuthenticator
    {
        private readonly string email;
        private readonly string identityProviderUri;
        private readonly List<KeyValuePair<string, string>> additionalQueryItems;

        /// <summary>
        /// Creates a new SSO authentication strategy
        /// </summary>
        /// <param name="clientId">the OAuth client id</param>
        /// <param name="clientSecret">the OAuth client secret</param>
        /// <param name="scope">space-separated string representing which permissions the application needs</param>
        /// <param name="redirectUri">the redirect URI that will be called when completing the authentication. This must match the redirect URI registered to your clientId.</param>
        /// <param name="email">the webex email address of the SSO user.</param>
        /// <param name="identityProviderUri">the URI that will handle authentication claims with webex service on behalf of the hosting application.</param>
        /// <param name="additionalQueryItems">a collection of additional *URLQueryItem* to be appended to the identityProviderUri.</param>
        public SSOAuthenticator(string clientId, string clientSecret, string scope, string redirectUri, string email, string identityProviderUri, List<KeyValuePair<string, string>> additionalQueryItems) 
            : base(clientId, clientSecret, scope, redirectUri)
        {
            this.email = email;
            this.identityProviderUri = identityProviderUri;
            this.additionalQueryItems = additionalQueryItems;
        }

        /// <summary>
        /// Overrides the AuthorizationUrl by taking the original url and redirecting the request through the
        /// provided identity provider uri. Once the identity provider has validated the claim with Cisco Services it will
        /// redirect back to continue a slimmed down version of oAuth authentication flow which has prefilled the user webex
        /// id.
        /// This flow only interacts with the user if they need to explicitly need to provide permissions to allow webex to
        /// use their account.
        /// </summary>
        public new string AuthorizationUrl
        {
            get
            {
                UriBuilder newUrl = new UriBuilder(this.identityProviderUri);
                string originalUrl = SCFCore.Instance.m_core.getAuthorizationUrl(this.email);
                string queryToAppend = string.Format("returnTo" + "=" + Uri.EscapeDataString(originalUrl));
                if (newUrl.Query != null && newUrl.Query.Length > 1)
                {
                    newUrl.Query = newUrl.Query.Substring(1) + "&" + queryToAppend;
                }
                else
                {
                    newUrl.Query = queryToAppend;
                }

                if (additionalQueryItems != null)
                {
                    foreach (var item in additionalQueryItems)
                    {
                        queryToAppend = string.Format(Uri.EscapeDataString(item.Key) + "=" + Uri.EscapeDataString(item.Value));
                        if (newUrl.Query != null && newUrl.Query.Length > 1)
                        {
                            newUrl.Query = newUrl.Query.Substring(1) + "&" + queryToAppend;
                        }
                        else
                        {
                            newUrl.Query = queryToAppend;
                        }
                    }
                }

                return newUrl.Uri.AbsoluteUri;
            }
        }
    }
}
