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
using Newtonsoft.Json;
using SparkNet;

namespace WebexSDK
{
    /// <summary>
    /// A [JSON Web Token](https://jwt.io/introduction) (JWT) based authentication strategy
    /// is to be used to authenticate a guest user on Cisco Webex. 
    /// </summary>
    /// <seealso cref="IAuthenticator" />
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class JWTAuthenticator : IAuthenticator
    {
        private bool isRegisteredToCore = false;
        private string jwt;
        private JWTAccessToken jwtAccessTokenStore;
        private JWTAuthClient client;
        private bool isAuthorized;
        internal bool MercuryConnected { get; set; }
        private SparkNet.CoreFramework m_core;

        private event Action<WebexApiEventArgs> AuthorizeAction;


        /// <summary>
        /// Initializes a new instance of the <see cref="JWTAuthenticator"/> class.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public JWTAuthenticator()
        {
            this.isRegisteredToCore = false;
            this.jwt = null;
            this.client = new JWTAuthClient();
            this.jwtAccessTokenStore = null;
            this.isAuthorized = false;

            RegisterToCore();
        }

        private void RegisterToCore()
        {
            if (isRegisteredToCore)
            {
                return;
            }
            m_core = SCFCore.Instance.m_core;
            m_core.m_CallbackEvent += OnCoreCallBack;

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
            if (SCFCore.Instance.m_core.getValue("jwtKey", ref this.jwt) == false
                || this.jwt == null
                || this.jwt.Length == 0)
            {
                completionHandler?.Invoke(new WebexApiEventArgs(false, null));
                return;
            }
            AuthorizeWith(jwt, completionHandler);
        }

        /// <summary>
        /// JWT Authenticate
        /// </summary>
        /// <param name="jwt">The new JSON Web Token to use</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void AuthorizeWith(string jwt, Action<WebexApiEventArgs> completionHandler = null)
        {
            // store jwt
            this.jwt = jwt;
            if (SCFCore.Instance.m_core.setValue("jwtKey", jwt) == false)
            {
                SdkLogger.Instance.Error("store jwt failed.");
                completionHandler?.Invoke(new WebexApiEventArgs(false, null));
                return;
            }

            if (!isRegisteredToCore)
            {
                RegisterToCore();
            }

            // get access token and login core
            AccessToken(response =>
            {
                if (response.IsSuccess == true)
                {
                    //core login
                    m_core.loginWithAccessToken(jwtAccessTokenStore.token, "", jwtAccessTokenStore.tokenExpirationSinceNow);
                    AuthorizeAction = completionHandler;
                    return;
                }
                else
                {
                    completionHandler?.Invoke(new WebexApiEventArgs(false, null));
                }
            });
        }

        /// <summary>
        /// Deauthorizes the current user and clears any persistent state with regards to the current user.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public void Deauthorize()
        {
            jwtAccessTokenStore = null;
            isAuthorized = false;
            SCFCore.Instance.m_core.setValue("jwtKey", "");

            UnRegisterToCore();
            SCFCore.Instance.UnLoad();
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

            SdkLogger.Instance.Debug("get an new access token");


            // check JWT if valid.
            if (null == GetUnexpiredJwt())
            {
                SdkLogger.Instance.Error("the jwt has expired.");
                completionHandler(new WebexApiEventArgs<string>(false, null, null));
                return;
            }

            // Fetch access token
            this.client.FetchTokenFromJWTAsync(this.jwt, this, (response =>
            {
                // Success Get AccessToken
                if (response != null && response.IsSuccess == true && response.Data != null)
                {
                    // Store AccessToken
                    string rspToken = response.Data.Token;
                    int rspExpiresIn = response.Data.ExpiresIn;

                    this.jwtAccessTokenStore = new JWTAccessToken(rspToken, rspExpiresIn);

                    // Set to Core
                    if (isAuthorized)
                    {
                        SetAccessTokenToCore(rspToken, rspExpiresIn);
                    }

                    SdkLogger.Instance.Info("get jwt access token success.");
                    // Callback to User
                    completionHandler(new WebexApiEventArgs<string>(true, null, this.jwtAccessTokenStore.token));
                    return;
                }

                SdkLogger.Instance.Error("fetch jwt token failed");
                // Callback to User
                completionHandler(new WebexApiEventArgs<string>(false, null, null));
                return;
            }));
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
            SdkLogger.Instance.Debug("get an new access token");

            // check JWT if valid.
            if (null == GetUnexpiredJwt())
            {
                SdkLogger.Instance.Error("the jwt has expired.");
                completionHandler(new WebexApiEventArgs<string>(false, null, null));
                return;
            }

            // Fetch access token
            this.client.FetchTokenFromJWTAsync(this.jwt, this, (response =>
            {
                // Success Get AccessToken
                if (response != null && response.IsSuccess == true && response.Data != null)
                {
                    // Store AccessToken
                    string rspToken = response.Data.Token;
                    int rspExpiresIn = response.Data.ExpiresIn;

                    this.jwtAccessTokenStore = new JWTAccessToken(rspToken, rspExpiresIn);

                    // Set to Core
                    if (isAuthorized)
                    {
                        SetAccessTokenToCore(rspToken, rspExpiresIn);
                    }

                    SdkLogger.Instance.Info("get jwt access token success.");
                    // Callback to User
                    completionHandler(new WebexApiEventArgs<string>(true, null, this.jwtAccessTokenStore.token));
                    return;
                }

                SdkLogger.Instance.Error("fetch jwt token failed");
                // Callback to User
                completionHandler(new WebexApiEventArgs<string>(false, null, null));
                return;
            }));
        }

        // get palyload from JWT
        private static Dictionary<string, string> PayloadFor(string jwt)
        {
            string[] segments = jwt.Split('.');
            if (segments.Length != 3)
            {
                SdkLogger.Instance.Error("jwt formate is invalid.");
                return null;
            }

            byte[] decodeResult;
            Dictionary<string, string> result;
            try
            {
                decodeResult = Base64UrlDecode(segments[1]);
                result = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString(decodeResult));
            }
            catch
            {
                SdkLogger.Instance.Error("deserialize jwt fail.");
                return null;
            }

            return result;
        }

        // from JWT spec
        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 1: output += "==="; break; // Three pad chars
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }

        private string GetUnexpiredJwt()
        {
            if (null == this.jwt)
            {
                SdkLogger.Instance.Error("jwt is null.");
                return null;
            }

            Dictionary<string, string> payload = PayloadFor(this.jwt);
            if (null == payload)
            {
                SdkLogger.Instance.Error("jwt payload is null");
                return null;
            }

            string expUtcTime;
            if (payload.TryGetValue("exp", out expUtcTime))
            {
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                DateTime expTime = startTime.AddSeconds(double.Parse(expUtcTime));

                if (expTime < DateTime.Now)
                {
                    SdkLogger.Instance.Error("JWT has expired at {0}", expTime);
                    return null;
                }
            }

            return this.jwt;
        }

        private string GetUnexpiredAccessToken()
        {
            if (false == this.isAuthorized || null == jwtAccessTokenStore)
            {
                return null;
            }

            if (jwtAccessTokenStore.tokenExpiration > DateTime.Now.AddMinutes(15))
            {
                return jwtAccessTokenStore.token;
            }

            return null;
        }

        private void SetAccessTokenToCore(string accessToken, int expiresIn)
        {
            m_core.refreshAccessToken(accessToken, expiresIn);
        }


        private void OnCoreCallBack(SCFEventType type, int error, string status)
        {        
            switch (type)
            {
                case SCFEventType.AccessTokenLoginCallback:
                    SdkLogger.Instance.Debug("event type: AccessTokenLoginCallback");
                    if (error == 0)
                    {
                        SdkLogger.Instance.Info("Log in success");
                        isAuthorized = true;
                        AuthorizeAction?.Invoke(new WebexApiEventArgs(true, null));
                        AuthorizeAction = null;
                    }
                    else
                    {
                        SdkLogger.Instance.Info("Log in failed");
                        isAuthorized = false;

                        AuthorizeAction?.Invoke(new WebexApiEventArgs(false, null));
                        AuthorizeAction = null;
                    }
                    break;

                default:
                    break;

            }
        }

    }
    internal class JWTAccessTokenInfo
    {
        public string Token { get; set; }

        public int ExpiresIn { get; set; }
    }
}
