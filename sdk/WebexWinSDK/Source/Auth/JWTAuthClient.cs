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
using System.Threading;

namespace WebexSDK
{
#pragma warning disable S101 // Types should be named in camel case
    internal class JWTAccessToken
#pragma warning restore S101 // Types should be named in camel case
    {
        public readonly string token;
        public int tokenExpirationSinceNow;
        public readonly DateTime tokenExpiration;
        public readonly DateTime tokenCreationDate;

        public JWTAccessToken(string token, int tokenExpirationSinceNow)
        {
            this.token = token;
            this.tokenExpirationSinceNow = tokenExpirationSinceNow;
            tokenCreationDate = DateTime.Now;
            this.tokenExpiration = tokenCreationDate.AddSeconds(tokenExpirationSinceNow);
        }

    }

#pragma warning disable S101 // Types should be named in camel case
    internal class JWTAuthClient
#pragma warning restore S101 // Types should be named in camel case
    {
        public void FetchTokenFromJWTAsync(string jwt, IAuthenticator authenticator,Action<WebexApiEventArgs<JWTAccessTokenInfo>> completionHandler)
        {
            var request = new ServiceRequest(authenticator)
            {
                Method = HttpMethod.POST,
                Resource = "jwt/login"
            };
            request.AddHeaders("Authorization", jwt);
            request.AddBodyParameters("Content-Type", "text/plain");
            request.AddBodyParameters("Cache-Control", "no-cache");
            request.AddBodyParameters("Accept-Encoding", "none");

            request.ExecuteAuth<JWTAccessTokenInfo>((response) =>
            {
                completionHandler(response);
            });

        }
    }
}
