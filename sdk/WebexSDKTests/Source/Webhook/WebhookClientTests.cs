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

namespace WebexSDK.Tests
{
    [TestClass()]
    public class WebhookClientTests
    {
        private WebexTestFixture fixture;
        private Webex webex;
        private WebhookClient webhooks;
        private Room myRoom;
        private Webhook myWebHook;


        [TestInitialize]
        public void SetUp()
        {
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            //webex = fixture.webex;
            webex = fixture.CreateWebex();
            Assert.IsNotNull(webex);

            webhooks = webex.Webhooks;
            Assert.IsNotNull(webhooks);

            myRoom = fixture.CreateRoom("test room");
            myWebHook = CreateWebHook();
            Assert.IsNotNull(myWebHook);
            Assert.IsNotNull(myWebHook.Id);
        }


        [TestCleanup]
        public void TearDown()
        {
            if (myRoom != null)
            {
                fixture.DeleteRoom(myRoom.Id);
            }
            if (myWebHook != null)
            {
                DeleteWebHook(myWebHook.Id);
            }
        }

        [TestMethod()]
        public void ListTest()
        {
            var list = ListWebHook();
            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count >= 1);
        }

        [TestMethod()]
        public void ListByMaxTest()
        {
            var newWebhook = CreateWebHook();
            var list = ListWebHook(1);
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod()]
        public void CreateTest()
        {
            var newWebhook = CreateWebHook();
            Assert.IsNotNull(newWebhook);
            Assert.IsNotNull(newWebhook.Id);
        }

        [TestMethod()]
        public void GetTest()
        {
            var result = GetWebHook(myWebHook.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(myWebHook.Id, result.Id);
            Assert.AreEqual(myWebHook.Name, result.Name);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            string updatedName = "update webhook";
            string updatedTargeUrl = "https://example.com/updated_test_webhook";
            var updated = UpdateWebHook(myWebHook.Id, updatedName, updatedTargeUrl);
            Assert.AreEqual(updatedName, updated.Name);
            Assert.AreEqual(updatedTargeUrl, updated.TargetUrl);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            var newWebhook = CreateWebHook();
            Assert.IsNotNull(newWebhook);
            Assert.IsTrue(DeleteWebHook(newWebhook.Id));
            Assert.IsNull(GetWebHook(newWebhook.Id));  
        }

        [TestMethod()]
        public void DeleteInvalidIdTest()
        {
            Assert.IsFalse(DeleteWebHook("abc"));           
        }

        private Webhook CreateWebHook()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Webhook>();
            webhooks.Create("test webhook", "https://example.com/test_webhook", "messages", "created", string.Format("roomId=" + myRoom.Id),null, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess == true)
            {
                return response.Data;
            }

            return null;
        }

        private List<Webhook> ListWebHook(int? max = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Webhook>>();
            webhooks.List(max, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess == true)
            {
                return response.Data;
            }

            return null;
        }

        private Webhook GetWebHook(string webhookId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Webhook>();
            webhooks.Get(webhookId, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess == true)
            {
                return response.Data;
            }

            return null;
        }

        private Webhook UpdateWebHook(string webhookId, string name, string targetUrl)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Webhook>();
            webhooks.Update(webhookId, name, targetUrl, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess == true)
            {
                return response.Data;
            }

            return null;
        }

        private bool DeleteWebHook(string webhookId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
            webhooks.Delete(webhookId, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                return false;
            }

            if (response.IsSuccess == true)
            {
                return true;
            }

            return false;
        }

    }
}