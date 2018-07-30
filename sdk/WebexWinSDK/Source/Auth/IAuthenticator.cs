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
    /// An interface for generic authentication strategies in Cisco Webex. Each authentication strategy
    /// is responsible for providing an accessToken used throughout this SDK.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public interface IAuthenticator
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IAuthenticator"/> is authorized.
        /// This may not mean the user has a valid
        /// access token yet, but the authentication strategy should be able to obtain one without
        /// further user interaction.
        /// </summary>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        void Authorized(Action<WebexApiEventArgs> completionHandler);

        /// <summary>
        /// Deauthorizes the current user and clears any persistent state with regards to the current user.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        void Deauthorize();

        /// <summary>
        /// Returns an access token of this authenticator.
        /// This may involve long-running operations such as service calls,
        /// but may also return immediately. The application should not make assumptions about how quickly this completes.
        /// If the access token could not be retrieved then the completion handler will be called with null.
        /// </summary>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        void AccessToken(Action<WebexApiEventArgs<string>> completionHandler);

        /// <summary>
        /// Returns an new access token of this authenticator.
        /// This may involve long-running operations such as service calls.
        /// If the access token could not be retrieved then the completion handler will be called with null.
        /// </summary>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.7</remarks>
        void RefreshToken(Action<WebexApiEventArgs<string>> completionHandler);

    }
}
