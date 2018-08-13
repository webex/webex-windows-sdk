using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebexSDK;
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
using System.Threading;
using System.Configuration;

namespace WebexSDK.Tests
{
    [TestClass()]
    public class OAuthAuthenticatorTests
    {
        string clientId = ConfigurationManager.AppSettings["ClientID"] ?? "";
        string clientSecret = ConfigurationManager.AppSettings["ClientSecret"] ?? "";
        string redirectUri = ConfigurationManager.AppSettings["RedirectURL"] ?? "";
        string scope = "spark: all";

        [TestMethod()]
        public void OAuthAuthenticatorTest()
        {
            var auth = new OAuthAuthenticator(clientId, clientSecret, scope, redirectUri);
            Assert.IsNotNull(auth);

            string authUrl = "https://idbroker.webex.com/idb/oauth2/v1/authorize?cisKeepMeSignedInOption=1&response_type=code&client_id=C452b978600b789f41d65b6a017f7f8e10c14925af17b1befac2293d107b943ac&redirect_uri=WebexSdkWinUnitTest%3A%2F%2Fredirect";
            Assert.IsTrue(auth.AuthorizationUrl.StartsWith(authUrl));
        }

        [TestMethod()]
        public void AuthorizeTestNullAuthCode()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();

            var auth = new OAuthAuthenticator(clientId, clientSecret, scope, redirectUri);

            auth.Authorize(null, r=>
            {
                response = r;
                completion.Set();
                
            });

            if (!completion.WaitOne(30000))
            {
                Assert.Fail();
            }

            Assert.IsFalse(response.IsSuccess);
        }

        [TestMethod()]
        public void AuthorizeTestInvalidAuthCode()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();

            var auth = new OAuthAuthenticator(clientId, clientSecret, scope, redirectUri);

            auth.Authorize("a.b.c", r =>
            {
                response = r;
                completion.Set();

            });

            if (!completion.WaitOne(30000))
            {
                Assert.Fail();
            }

            Assert.IsFalse(response.IsSuccess);
        }

        [TestMethod()]
        public void AuthorizeTest()
        {
            var auth = new OAuthAuthenticator(clientId, clientSecret, scope, redirectUri);

            string authurl = auth.AuthorizationUrl;
            Console.WriteLine("you need mannul copy the authurl to browser, to get auth code. and then paste the following method parapeter");
            Console.WriteLine("auth url: {0}", authurl);
            //string authcode = "";

            //auth.authorize(authcode, r =>
            //{
            //    Assert.IsTrue(r.Success);
            //});
        }

        [TestMethod()]
        public void AccessTokenTestNullToken()
        {
            var auth = new OAuthAuthenticator(clientId, clientSecret, scope, redirectUri);
            auth.AccessToken(r=>
            {
                Assert.IsFalse(r.IsSuccess);
            });
            
        }

        //[TestMethod()]
        //public void DeauthorizeTest()
        //{
        //    var auth = new OAuthAuthenticator(clientId, clientSecret, scope, redirectUri);
        //    auth.Deauthorize();
        //}
    }
}
