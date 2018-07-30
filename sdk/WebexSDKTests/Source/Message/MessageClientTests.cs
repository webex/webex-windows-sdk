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
using System.IO;

namespace WebexSDK.Tests
{
    [TestClass()]
    public class MessageClientTests
    {
        private static WebexTestFixture fixture;
        private static Webex webex;
        private static MessageClient messages;
        private static Person self;
        private static TestUser other;
        private string text = "test text.";
        private static string fileUrl = null;
        private static Room myRoom;
        private static readonly string testFixtureApp = "TestFixtureApp";

        private readonly static string calleeAddress = ConfigurationManager.AppSettings["TestFixtureAppAddress01"] ?? "";
        private static string calleePersonEmail;
        private static string calleePersonId;

        private Message recvedMessage = null;
        private string deletedMessageId = null;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            Console.WriteLine("ClassSetup");
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            webex = fixture.CreateWebexbyJwt();
            Assert.IsNotNull(webex);

            messages = webex.Messages;
            Assert.IsNotNull(messages);

            self = GetMe();

            other = fixture.CreatUser();

            myRoom = CreateRoom("my test room");
            Assert.IsNotNull(myRoom);

            if (StringExtention.GetHydraIdType(calleeAddress) == StringExtention.HydraIdType.People)
            {
                calleePersonId = calleeAddress;
                calleePersonEmail = GetSpecificPerson(calleePersonId)?.Emails[0];
            }
            else if (calleeAddress.Contains("@"))
            {
                calleePersonEmail = calleeAddress;
                calleePersonId = GetPerson(calleeAddress).Id;
            }
            else
            {
            }
            Assert.IsNotNull(CreateMembership(myRoom.Id, null, calleePersonId, false));

            fileUrl = Directory.GetCurrentDirectory() + "\\Resources\\" + "WebexTeams.jpg";

            Thread.Sleep(30000);
        }

        [ClassCleanup]
        public static void ClassTearDown()
        {
            Console.WriteLine("ClassTearDown");
            if (myRoom != null)
            {
                DeleteRoom(myRoom.Id);
                myRoom = null;
            }

            fixture = null;
            webex = null;
            messages = null;
            myRoom = null;
        }

        [TestInitialize]
        public void SetUp()
        {
            messages.OnEvent += ReceiveMessageEvent;
        }


        [TestCleanup]
        public void TearDown()
        {
            messages.OnEvent -= ReceiveMessageEvent;
            recvedMessage = null;
            deletedMessageId = null;
    }

        [TestMethod()]
        public void ListTest()
        {
            var msg = PostMsg(myRoom.Id, null, text);
            Validate(msg);
            Thread.Sleep(60000);
            var list = ListMsg(myRoom.Id,null, DateTime.UtcNow);
            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod()]
        public void ListByMaxTest()
        {
            PostMsg(myRoom.Id, null, text);
            PostMsg(myRoom.Id, null, text);
            PostMsg(myRoom.Id, null, text);
            Thread.Sleep(60000);
            var list = ListMsg(myRoom.Id, null, DateTime.UtcNow, 2);
            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count == 2);
        }

        [TestMethod()]
        public void ListPersistMsgTest()
        {
            var msg = PostMsg(null, calleePersonId, text);
            Validate(msg);

            var list = ListMsg(msg.RoomId, null, msg.Id, 1000);
            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod()]
        public void ListInvalidRoomIdTest()
        {
            var msg = PostMsg(myRoom.Id, null, text);
            Validate(msg);
            var list = ListMsg("abc", null, null);
            Assert.IsNull(list);
        }

        [TestMethod()]
        public void ListByBeforeTimeTest()
        {
            var msg1 = PostMsg(myRoom.Id, null, text);
            Assert.IsNotNull(msg1);
            Console.WriteLine("post first Msg at: {0}", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFZ"));
            Thread.Sleep(60000);
            var time1 = DateTime.UtcNow;
            Thread.Sleep(60000);
            var msg2 = PostMsg(myRoom.Id, null, text);
            Assert.IsNotNull(msg2);
            Console.WriteLine("post second Msg at: {0}", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFZ"));

            var list = ListMsg(myRoom.Id, null, time1, null);
            Console.WriteLine("list message before {0}", time1);

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count > 0);
            Assert.IsNotNull(list.Find(item => item.Id == msg1.Id));
            Assert.IsNull(list.Find(item => item.Id == msg2.Id));
        }

        [TestMethod()]
        public void ListByBeforeMessageIdTest()
        {
            var msg1 = PostMsg(myRoom.Id, null, "msg1", null);
            var msg2 = PostMsg(myRoom.Id, null, "msg2", null);
           

            var list = ListMsg(myRoom.Id, null, msg2.Id, null);
            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count > 0);
            Assert.IsNotNull(list.Find(item => item.Id == msg1.Id));
            Assert.IsNull(list.Find(item => item.Id == msg2.Id));
        }

        [TestMethod()]
        public void ListByMentionedPeopleTest()
        {
            var list = ListMsg(myRoom.Id, "me", null);
            Assert.IsNotNull(list);
        }


        [TestMethod()]
        public void PostToPersonByIdWithTextTest()
        {
            var msg = PostMsg(null, calleePersonId, text, null, null);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
        }

        //###Comment temporary for Network unstable
        //[TestMethod()]
        //public void PostToPersonByIdWithFileTest()
        //{
        //    var files = new List<LocalFile>();
        //    files.Add(new LocalFile()
        //    {
        //        Path = fileUrl,
        //    });
        //    var msg = PostMsg(null, calleePersonId, null, null, files);
        //    Validate(msg);
        //    Assert.IsNotNull(msg.Files);
        //}

        //[TestMethod()]
        //public void PostToPersonByIdWithTextAndFileTest()
        //{
        //    var files = new List<LocalFile>();
        //    files.Add(new LocalFile()
        //    {
        //        Path = fileUrl,
        //        Name = Path.GetFileName(fileUrl),
        //        Mime = "image/jpeg",
        //        Size = (ulong)new System.IO.FileInfo(fileUrl).Length,
        //        LocalThumbnail = new LocalFile.Thumbnail()
        //        {
        //            Path = fileUrl,
        //            Width =512,
        //            Height =512,
        //            Size = (ulong)new System.IO.FileInfo(fileUrl).Length,
        //            Mime = "image/jpeg",
        //        }
        //    });
        //    var msg = PostMsg(null, calleePersonId,text, null, files);
        //    Validate(msg);

        //    Assert.AreEqual(text, msg.Text);
        //    Assert.IsNotNull(msg.Files);
        //    Assert.IsTrue(msg.Files.Count > 0);
        //    Assert.AreEqual(files[0].Name, msg.Files[0].Name);
        //    Assert.AreEqual(files[0].Size, msg.Files[0].Size);
        //    Assert.AreEqual(files[0].Mime, msg.Files[0].Mime);
        //    Assert.IsNotNull(msg.Files[0].RemoteThumbnail);
        //    Assert.AreEqual(files[0].LocalThumbnail.Width, msg.Files[0].RemoteThumbnail.Width);
        //    Assert.AreEqual(files[0].LocalThumbnail.Height, msg.Files[0].RemoteThumbnail.Height);
        //    //Assert.AreEqual(files[0].LocalThumbnail.Mime, msg.Files[0].RemoteThumbnail.Mime);
        //}

        [TestMethod()]
        public void PostToPersonByInvalidIdTest()
        {
            var msg = PostMsg(null, "abc", text);
            Assert.IsNull(msg);
        }
        //Guset issuer user has not vailid email.
        [TestMethod()]
        public void PostToPersonByEmailTest()
        {
            var msg = PostMsg(null, calleePersonEmail, text);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
        }
        //###Comment temporary for Network unstable
        //[TestMethod()]
        //public void PostToPersonByEmailWithFileTest()
        //{
        //    var files = new List<LocalFile>();
        //    files.Add(new LocalFile()
        //    {
        //        Path = fileUrl,

        //    });
        //    var msg = PostMsg(null, calleePersonEmail, null, null, files);
        //    Validate(msg);
        //    Assert.IsNotNull(msg.Files);
        //}

        //[TestMethod()]
        //public void PostToPersonByEmailWithTextAndFileTest()
        //{
        //    var files = new List<LocalFile>();
        //    files.Add(new LocalFile()
        //    {
        //        Path = fileUrl,

        //    });
        //    var msg = PostMsg(null, calleePersonEmail, text, null, files);
        //    Validate(msg);
        //    Assert.AreEqual(text, msg.Text);
        //    Assert.IsNotNull(msg.Files);
        //}

        [TestMethod()]
        public void PostToPersonFirstTimeTest()
        {
            var msg = PostMsg(null, other.PersonId, text);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
        }

        [TestMethod()]
        public void PostToRoomWithTextTest()
        {
            var msg = PostMsg(myRoom.Id, null, text);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
        }
        //###Comment temporary for Network unstable
        //[TestMethod()]
        //public void PostToRoomWithFileTest()
        //{
        //    var files = new List<LocalFile>();
        //    files.Add(new LocalFile()
        //    {
        //        Path = fileUrl,
        //    });
        //    var msg = PostMsg(myRoom.Id, null, null, null, files);
        //    Validate(msg);
        //    Assert.IsNotNull(msg.Files);
        //}

        //[TestMethod()]
        //public void PostToRoomWithTextAndFileTest()
        //{
        //    var files = new List<LocalFile>();
        //    files.Add(new LocalFile()
        //    {
        //        Path = fileUrl,
        //    });
        //    var msg = PostMsg(myRoom.Id, null, text, null, files);
        //    Validate(msg);
        //    Assert.AreEqual(text, msg.Text);
        //    Assert.IsNotNull(msg.Files);
        //}

        [TestMethod()]
        public void PostToRoomWithMentionAllTest()
        {
            var mentions = new List<Mention>();
            mentions.Add(new MentionAll());
            var msg = PostMsg(myRoom.Id, null, text, mentions);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
            Assert.AreEqual(false, msg.IsSelfMentioned);
        }

        [TestMethod()]
        public void PostToRoomWithMentionPersonTest()
        {
            var mentions = new List<Mention>();
            mentions.Add(new MentionPerson(calleePersonId));
            var msg = PostMsg(myRoom.Id, null, text, mentions);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
            Assert.AreEqual(false, msg.IsSelfMentioned);
        }

        [TestMethod()]
        public void PostToRoomByInvalidIdTest()
        {
            var msg = PostMsg("abc", null, text);
            Assert.IsNull(msg);
        }

        [TestMethod()]
        public void GetByMessageIdTest()
        {
            var msg = PostMsg(null, calleePersonId, text);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
            var result = GetMsgByMessageId(msg.Id);
            Validate(result);
            Assert.AreEqual(msg.Id, result.Id);
            Assert.AreEqual(msg.Text, result.Text);
        }

        [TestMethod()]
        public void GetByRoomIdAndMessageIdTest()
        {
            var msg = PostMsg(null, calleePersonId, text);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
            var result = GetMsgByRoomIdAndMessageId(msg.RoomId,msg.Id);
            Validate(result);
            Assert.AreEqual(msg.Id, result.Id);
            Assert.AreEqual(msg.Text, result.Text);
        }

        [TestMethod()]
        public void GetByInvalidIdTest()
        {
            var msg = PostMsg(null, calleePersonId, text);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
            var result = GetMsgByMessageId("abc");
            Assert.IsNull(result);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            var msg = PostMsg(null, calleePersonId, text);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
            Assert.IsTrue(DeleteMsg(msg.Id));
            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(msg.Id, deletedMessageId);
            Assert.AreEqual("", GetMsgByMessageId(msg.Id).Text);// still can get this message but text is null.
        }

        [TestMethod()]
        public void DeleteByBadIdTest()
        {
            var msg = PostMsg(null, calleePersonId, text);
            Validate(msg);
            Assert.AreEqual(text, msg.Text);
            Assert.IsFalse(DeleteMsg("abc"));
            Assert.IsFalse(DeleteMsg(""));
            Assert.IsFalse(DeleteMsg(null));
        }
        private void ReceiveMessageEvent(MessageEvent e)
        {
            if (e is MessageArrived)
            {
                var messageArrived = e as MessageArrived;
                recvedMessage = messageArrived?.Message;
                MessageHelper.BreakLoop();
            }
            else if (e is MessageDeleted)
            {
                var messageDeleted = e as MessageDeleted;
                deletedMessageId = messageDeleted.MessageId;
                MessageHelper.BreakLoop();
            }
        }

        [TestMethod()]
        public void ReceiveDirectMessageTest()
        {    
            MessageHelper.SetTestMode_RemoteSendDirectMessage(testFixtureApp, self.Id, text);
            MessageHelper.RunDispatcherLoop();

            Assert.IsNotNull(recvedMessage);
            Validate(recvedMessage);
            Assert.AreEqual(text, recvedMessage.Text);
            Assert.IsFalse(recvedMessage.IsSelfMentioned);
        }
        //###Comment temporary for Network unstable
        //[TestMethod()]
        //public void ReceiveDirectMessageWithFilesTest()
        //{
        //    MessageHelper.SetTestMode_RemoteSendDirectMessageWithFiles(testFixtureApp, self.Id, text);
        //    MessageHelper.RunDispatcherLoop();

        //    Assert.IsNotNull(recvedMessage);
        //    Validate(recvedMessage);
        //    Assert.AreEqual(text, recvedMessage.Text);
        //    Assert.IsNotNull(recvedMessage.Files);
        //    Assert.IsTrue(recvedMessage.Files.Count == 1);
        //    Assert.IsNotNull(recvedMessage.Files[0].Name);
        //    Assert.IsNotNull(recvedMessage.Files[0].Mime);
        //    Assert.IsTrue(recvedMessage.Files[0].Size > 0);

        //    Assert.IsFalse(recvedMessage.IsSelfMentioned);
        //}

        //###Comment temporary for Network unstable
        //[TestMethod()]
        //public void DownloadFileTest()
        //{
        //    MessageHelper.SetTestMode_RemoteSendDirectMessageWithFiles(testFixtureApp, self.Id, text);
        //    MessageHelper.RunDispatcherLoop();

        //    Assert.IsNotNull(recvedMessage);
        //    Validate(recvedMessage);
        //    Assert.AreEqual(text, recvedMessage.Text);
        //    Assert.IsNotNull(recvedMessage.Files);
        //    Assert.IsTrue(recvedMessage.Files.Count == 1);

        //    var file = recvedMessage.Files[0];
        //    string to = Directory.GetCurrentDirectory() + "\\";
        //    Assert.IsTrue(DownloadFile(file, to));
        //    Thread.Sleep(10000);
        //}

        //[TestMethod()]
        //public void DownloadThumbnailTest()
        //{
        //    MessageHelper.SetTestMode_RemoteSendDirectMessageWithFiles(testFixtureApp, self.Id, text);
        //    MessageHelper.RunDispatcherLoop();

        //    Assert.IsNotNull(recvedMessage);
        //    Validate(recvedMessage);
        //    Assert.AreEqual(text, recvedMessage.Text);
        //    Assert.IsNotNull(recvedMessage.Files);
        //    Assert.IsTrue(recvedMessage.Files.Count == 1);

        //    var file = recvedMessage.Files[0];
        //    DownloadThumbnail(file, null);
        //    Thread.Sleep(10000);
        //}

        [TestMethod()]
        public void ReceiveRoomMessageTest()
        {
            MessageHelper.SetTestMode_RemoteSendRoomMessage(testFixtureApp, myRoom.Id, text);
            MessageHelper.RunDispatcherLoop();

            Assert.IsNotNull(recvedMessage);
            Validate(recvedMessage);
            Assert.AreEqual(text, recvedMessage.Text);
            Assert.IsFalse(recvedMessage.IsSelfMentioned);
        }

        [TestMethod()]
        public void ReceiveRoomMessageWithMentionTest()
        {
            MessageHelper.SetTestMode_RemoteSendRoomMessageWithMention(testFixtureApp, myRoom.Id, text, self.Id);
            MessageHelper.RunDispatcherLoop();

            Assert.IsNotNull(recvedMessage);
            Validate(recvedMessage);
            Assert.AreEqual(text, recvedMessage.Text);
            Assert.IsTrue(recvedMessage.IsSelfMentioned);
        }
        [TestMethod()]
        public void ReceiveRoomMessageWithMentionAllTest()
        {
            MessageHelper.SetTestMode_RemoteSendRoomMessageWithMention(testFixtureApp, myRoom.Id, text, "ALL");
            MessageHelper.RunDispatcherLoop();

            Assert.IsNotNull(recvedMessage);
            Validate(recvedMessage);
            Assert.AreEqual(text, recvedMessage.Text);
            Assert.IsTrue(recvedMessage.IsSelfMentioned);
        }

        private void Validate(Message msg)
        {
            Assert.IsNotNull(msg);
            Assert.IsNotNull(msg.Id);
            Assert.IsNotNull(msg.PersonEmail);
            Assert.IsNotNull(msg.PersonId);
            Assert.IsNotNull(msg.RoomId);
            Assert.IsNotNull(msg.Text);
        }

        private Message PostMsg(string roomId, string person, string text, List<Mention>mentions=null, List<LocalFile>files=null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Message>();
            if (roomId != null)
            {
                webex.Messages.PostToRoom(roomId, text, mentions, files, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }
            else if (person != null)
            {
                webex.Messages.PostToPerson(person, text, files, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }
            else
            {
            }


            if (false == completion.WaitOne(30000))
            {
                Console.WriteLine("postMsg outof time");
                return null;
            }

            if (response.IsSuccess == true)
            {
                return response.Data;
            }

            Console.WriteLine($"postMsg fail {response.Error?.ErrorCode} {response.Error?.Reason}");

            return null;
        }

        private Message GetMsgByMessageId(string msgId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Message>();
            webex.Messages.Get(msgId, rsp =>
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

        private Message GetMsgByRoomIdAndMessageId(string roomId, string msgId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Message>();
            webex.Messages.Get(roomId, msgId, rsp =>
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

        private List<Message> ListMsg(string roomId, string mentionedPeople, DateTime before, int? max = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Message>>();

            webex.Messages.List(roomId, mentionedPeople, before, max, rsp =>
            {
                response = rsp;
                completion.Set();
            });
            if (false == completion.WaitOne(30000))
            {
                Console.WriteLine("listMsg outof time");
                return null;
            }

            if (response.IsSuccess == true)
            {
                return response.Data;
            }

            Console.WriteLine($"listMsg faile {response.Error?.ErrorCode} {response.Error?.Reason}");
            return null;
        }

        private List<Message> ListMsg(string roomId, string mentionedPeople, string beforeMessage, int? max = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Message>>();

            webex.Messages.List(roomId, mentionedPeople, beforeMessage, max, rsp =>
            {
                response = rsp;
                completion.Set();
            });
            if (false == completion.WaitOne(30000))
            {
                Console.WriteLine("listMsg outof time");
                return null;
            }

            if (response.IsSuccess == true)
            {
                return response.Data;
            }

            Console.WriteLine($"listMsg faile {response.Error?.ErrorCode} {response.Error?.Reason}");
            return null;
        }

        private bool DeleteMsg(string MsgId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();

            webex.Messages.Delete(MsgId, rsp =>
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

        private bool DownloadFile( RemoteFile file, string to)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();

            webex.Messages.DownloadFile(file, to, rsp =>
            {
                if (!rsp.IsSuccess || (rsp.IsSuccess && rsp.Data == 100))
                {
                    response = rsp;
                    completion.Set();
                }
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

        private bool DownloadThumbnail(RemoteFile file, string to)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();

            webex.Messages.DownloadThumbnail(file, to, rsp =>
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
        private static Room CreateRoom(string title)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Room>();
            webex.Rooms.Create(title, null, rsp =>
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

        private static bool DeleteRoom(string roomId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
            webex.Rooms.Delete(roomId, rsp =>
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
        private static Membership CreateMembership(string roomId, string email, string personId, bool isModerator = false)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Membership>();
            if (email != null)
            {
                webex.Memberships.CreateByPersonEmail(roomId, email, isModerator, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }
            else
            {
                webex.Memberships.CreateByPersonId(roomId, personId, isModerator, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }


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
        private static Person GetPerson(string email)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            webex.People.List(email, null, 1, rsp =>
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
                var persons = response.Data;
                if (persons != null && persons.Count > 0)
                {
                    return persons[0];
                }
                return null;
                
            }

            return null;

        }

        private static Person GetSpecificPerson(string personId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Person>();
            webex.People.Get(personId, rsp =>
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
        private static Person GetMe()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Person>();
            webex.People.GetMe(rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess)
            {
                return response.Data;
            }
            return null;
        }
    }
}