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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using SparkNet;

namespace WebexSDK
{
    /// <summary>
    /// An [OAuth](https://oauth.net/2/) based authentication strategy is to be used to authenticate a user on Cisco Webex. 
    /// </summary>
    /// See: [Cisco Webex Integration](https://developer.webex.com/authentication.html)
    /// <seealso cref="WebexSDK.IAuthenticator" />
    /// <remarks>Since: 0.1.0</remarks>
    public class OAuthAuthenticator : IAuthenticator
    {
        private SparkNet.CoreFramework m_core;
        private bool isRegisteredToCore = false;
        private AccessToken accessTokenStore;

        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string scope;
        private readonly string redirectUri;

        private bool isAuthorized;


        /// <summary>
        /// Gets a value indicating whether this <see cref="IAuthenticator"/> is authorized.
        /// This may not mean the user has a valid
        /// access token yet, but the authentication strategy should be able to obtain one without
        /// further user interaction.
        /// </summary>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Authorized(Action<WebexApiEventArgs> completionHandler)
        {
            if(isAuthorized)
            {
                completionHandler(new WebexApiEventArgs(true, null));
                return;
            }

            if (!isRegisteredToCore)
            {
                RegisterToCore();
            }
            AuthorizedAction = completionHandler;
            m_core.tryToLogin();
        }

        private event Action<WebexApiEventArgs> AuthorizedAction;
        private event Action<WebexApiEventArgs> AuthorizeAction;
        private event Action<WebexApiEventArgs<string>> AccessTokenAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthAuthenticator"/> class.
        /// </summary>
        /// <param name="clientId">the OAuth client id</param>
        /// <param name="clientSecret">the OAuth client secret</param>
        /// <param name="scope">space-separated string representing which permissions the application needs</param>
        /// <param name="redirectUri">the redirect URI that will be called when completing the authentication. This must match the redirect URI registered to your clientId.</param>
        /// - see: [Cisco Webex Integration](https://developer.webex.com/authentication.html)
        /// <remarks>Since: 0.1.0</remarks>
        public OAuthAuthenticator(string clientId, string clientSecret, string scope, string redirectUri)
        {
            this.isRegisteredToCore = false;
            this.isAuthorized = false;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.scope = scope;
            this.redirectUri = redirectUri;

            RegisterToCore();
        }

        /// <summary>
        /// Get authorization url
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string AuthorizationUrl
        {
            get { return m_core.getAuthorizationUrl(""); }
        }

        /// <summary>
        /// Authenticate process with authentication code, and browser is trigged by SDK users.
        /// </summary>
        /// <param name="authCode">The authentication code</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Authorize(string authCode, Action<WebexApiEventArgs> completionHandler = null)
        {
            if (null == authCode)
            {
                SdkLogger.Instance.Error("auth code is null.");
                if (completionHandler != null) completionHandler(new WebexApiEventArgs(false, null));
                return;
            }
            AuthorizeAction = completionHandler;

            if(!isRegisteredToCore)
            {
                RegisterToCore();
            }
            m_core.login(authCode, "");
        }

        /// <summary>
        /// Returns an access token of this authenticator.
        /// This may involve long-running operations such as service calls,
        /// but may also return immediately. The application should not make assumptions about how quickly this completes.
        /// If the access token could not be retrieved then the completion handler will be called with null.
        /// </summary>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void AccessToken(Action<WebexApiEventArgs<string>> completionHandler)
        {    
            // access token is valid now, just return it.
            string token = GetUnexpiredAccessToken();
            if (token != null)
            {
                completionHandler(new WebexApiEventArgs<string>(true, null, token));
                return;
            }

            this.AccessTokenAction = completionHandler;

            // fetch a new access token.
            m_core.requestRefreshAccessToken();

        }

        /// <summary>
        /// Returns an new access token of this authenticator.
        /// This may involve long-running operations such as service calls.
        /// If the access token could not be retrieved then the completion handler will be called with null.
        /// </summary>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.7</remarks>
        public void RefreshToken(Action<WebexApiEventArgs<string>> completionHandler)
        {
            this.AccessTokenAction = completionHandler;

            // fetch a new access token.
            m_core.requestRefreshAccessToken();
        }

        /// <summary>
        /// Deauthorizes the current user and clears any persistent state with regards to the current user.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public void Deauthorize()
        {
            this.accessTokenStore = null;
            this.isAuthorized = false;

            UnRegisterToCore();
            SCFCore.Instance.UnLoad();
        }

        private void RegisterToCore()
        {
            if (isRegisteredToCore)
            {
                return;
            }
            m_core = SCFCore.Instance.m_core;
            m_core.m_CallbackEvent += OnCoreCallBack;
            m_core.setApplicationIntegrationProperties(this.clientId, this.clientSecret, this.scope, this.redirectUri);

            isRegisteredToCore = true;
        }
        private void UnRegisterToCore()
        {
            if (!isRegisteredToCore)
            {
                return;
            }
            m_core.m_CallbackEvent -= OnCoreCallBack;
            m_core = null;
            isRegisteredToCore = false;
        }


        private void OnCoreCallBack(SCFEventType type, int error, string status)
        {
            SdkLogger.Instance.Debug("event type:{0}, error[{1}], status:{2}", type.ToString(), error, status);
            switch (type)
            {
                case SCFEventType.AccessTokenRefreshed:
                    if (error == 0)
                    {
                        string rspToken = null;
                        int rspExpiresIn = 0;
                        m_core.getAccessToken(ref rspToken, ref rspExpiresIn);

                        this.accessTokenStore = new AccessToken(rspToken, rspExpiresIn);

                        SdkLogger.Instance.Info("access token refreshed");

                        this.AccessTokenAction?.Invoke(new WebexApiEventArgs<string>(true, null, this.accessTokenStore.token));
                        this.AccessTokenAction = null;                   
                    }
                    else
                    {
                        SdkLogger.Instance.Error("access token failed");
                        this.AccessTokenAction?.Invoke(new WebexApiEventArgs<string>(false, null, null));
                        this.AccessTokenAction = null;

                    }
                    break;
                case SCFEventType.Login:
                
                    if (error == 0)
                    {
                        this.isAuthorized = true;  

                        string rspToken = null;
                        int rspExpiresIn = 0;
                        m_core.getAccessToken(ref rspToken, ref rspExpiresIn);

                        this.accessTokenStore = new AccessToken(rspToken, rspExpiresIn);
                        SdkLogger.Instance.Info("Log in success");

                        this.AuthorizeAction?.Invoke(new WebexApiEventArgs(true, null));
                        this.AuthorizeAction = null;

                    }
                    else
                    {
                        this.isAuthorized = false;
                        SdkLogger.Instance.Error("Log in failed. error:{0}, status:{1}", error, status);
     
                        this.AuthorizeAction?.Invoke(new WebexApiEventArgs(false, null));
                        this.AuthorizeAction = null;

                    }
                    break;
                case SCFEventType.RefreshTokenLoginCallback:
                    if (error == 0)
                    {
                        this.isAuthorized = true;

                        string rspToken = null;
                        int rspExpiresIn = 0;
                        m_core.getAccessToken(ref rspToken, ref rspExpiresIn);

                        this.accessTokenStore = new AccessToken(rspToken, rspExpiresIn);
                        SdkLogger.Instance.Info("RefreshTokenLogin in success");

                        this.AuthorizedAction?.Invoke(new WebexApiEventArgs(true, null));
                        this.AuthorizedAction = null;

                    }
                    else
                    {
                        this.isAuthorized = false;
                        SdkLogger.Instance.Info("RefreshTokenLogin in failed. error:{0}, status:{1}", error, status);
                        this.AuthorizedAction?.Invoke(new WebexApiEventArgs(false, null));
                        this.AuthorizedAction = null;
                    }
                    break;
                default:
                    break;
            }
            
        }

        private string GetUnexpiredAccessToken()
        {
            if (!this.isAuthorized || null == accessTokenStore)
            {
                return null;
            }

            if (accessTokenStore.tokenExpiration > DateTime.Now.AddMinutes(15))
            {
                return accessTokenStore.token;
            }

            return null;
        }
    }


    internal class AccessToken
    {
        public readonly string token;
        public readonly DateTime tokenExpiration;
        public readonly DateTime tokenCreationDate;


        public AccessToken() { }
        public AccessToken(string token, int tokenExpirationSinceNow)
        {
            this.token = token;
            tokenCreationDate = DateTime.Now;
            this.tokenExpiration = tokenCreationDate.AddSeconds(tokenExpirationSinceNow);
        }

    }
}
