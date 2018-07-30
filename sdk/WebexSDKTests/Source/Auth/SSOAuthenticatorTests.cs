using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using WebexSDK;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebexSDK.Tests
{

    [TestClass()]
    public class SSOAuthenticatorTests
    {
        string clientId = ConfigurationManager.AppSettings["ClientID"] ?? "";
        string clientSecret = ConfigurationManager.AppSettings["ClientSecret"] ?? "";
        string redirectUri = ConfigurationManager.AppSettings["RedirectURL"] ?? "";
        string scope = "webex: all";

        [TestMethod()]
        public void SSOAuthenticatorTest()
        {
            string email = "111@cc.cc";
            string identityProviderUri = "www.baidu.com";
            List<KeyValuePair<string, string>> additionalQueryItems = new List<KeyValuePair<string, string>>();
            additionalQueryItems.Add(new KeyValuePair<string, string>("key1", "value1://value1"));
            additionalQueryItems.Add(new KeyValuePair<string, string>("key2", "value2"));

            var auth = new SSOAuthenticator(clientId, clientSecret, scope, redirectUri, email, identityProviderUri, additionalQueryItems);
            Assert.IsNotNull(auth);

            string checkStr1 = "http://www.baidu.com/?returnTo=https%3A%2F%2Fidbroker.webex.com%2Fidb%2Foauth2%2Fv1%2Fauthorize%3FcisKeepMeSignedInOption%3D1%26response_type%3Dcode%26";
            string checkStr2 = Uri.EscapeDataString("client_id=" + Uri.EscapeDataString(clientId) + "&redirect_uri=" + Uri.EscapeDataString(redirectUri));
            string checkStr3 = "email%3D111%40cc.cc&key1=value1%3A%2F%2Fvalue1&key2=value2";
            string url = auth.AuthorizationUrl;
            Assert.IsTrue(url.StartsWith(checkStr1));
            Assert.IsTrue(url.Contains(checkStr2));
            Assert.IsTrue(url.Contains(checkStr3));
        }
        [TestMethod()]
        public void SSOAuthenticatorTestNullQueryItems()
        {
            string email = "111@cc.cc";
            string identityProviderUri = "www.baidu.com";

            var auth = new SSOAuthenticator(clientId, clientSecret, scope, redirectUri, email, identityProviderUri, null);
            Assert.IsNotNull(auth);

            string checkStr1 = "http://www.baidu.com/?returnTo=https%3A%2F%2Fidbroker.webex.com%2Fidb%2Foauth2%2Fv1%2Fauthorize%3FcisKeepMeSignedInOption%3D1%26response_type%3Dcode%26";
            string checkStr2 = Uri.EscapeDataString("client_id=" + Uri.EscapeDataString(clientId) + "&redirect_uri=" + Uri.EscapeDataString(redirectUri));
            string checkStr3 = "email%3D111%40cc.cc";
            string url = auth.AuthorizationUrl;
            Assert.IsTrue(url.StartsWith(checkStr1));
            Assert.IsTrue(url.Contains(checkStr2));
            Assert.IsTrue(url.Contains(checkStr3));
        }

        [TestMethod()]
        public void SSOAuthenticatorTestWhenidPhasQueryParameter()
        {
            string email = "111@cc.cc";
            string identityProviderUri = "http://www.baidu.com/?key1=value1";

            var auth = new SSOAuthenticator(clientId, clientSecret, scope, redirectUri, email, identityProviderUri, null);
            Assert.IsNotNull(auth);

            string checkStr1 = "http://www.baidu.com/?key1=value1&returnTo=https%3A%2F%2Fidbroker.webex.com%2Fidb%2Foauth2%2Fv1%2Fauthorize%3FcisKeepMeSignedInOption%3D1%26response_type%3Dcode%26";
            string checkStr2 = Uri.EscapeDataString("client_id=" + Uri.EscapeDataString(clientId) + "&redirect_uri=" + Uri.EscapeDataString(redirectUri));
            string checkStr3 = "email%3D111%40cc.cc";
            string url = auth.AuthorizationUrl;
            Assert.IsTrue(url.StartsWith(checkStr1));
            Assert.IsTrue(url.Contains(checkStr2));
            Assert.IsTrue(url.Contains(checkStr3));
        }
    }
}