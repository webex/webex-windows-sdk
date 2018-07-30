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
    public class MembershipClientTests
    {
        private WebexTestFixture fixture;
        private Webex webex;
        private MembershipClient memberships;
        private string roomId;
        private TestUser other;
        private Membership membership;

        [TestInitialize]
        public void SetUp()
        {
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            //webex = fixture.webex;
            webex = fixture.CreateWebex();
            Assert.IsNotNull(webex);

            memberships = webex.Memberships;
            Assert.IsNotNull(memberships);

            if (other == null)
            {
                other = fixture.CreatUser();
            }
            var room = fixture.CreateRoom("test room");
            Assert.IsNotNull(room);
            roomId = room.Id;
        }


        [TestCleanup]
        public void TearDown()
        {
            if (membership != null)
            {
                DeleteMembership(membership.Id);
            }
            if (roomId != null)
            {
                fixture.DeleteRoom(roomId);
            }

            
        }

        [TestMethod()]
        public void ListTest()
        {
            var list = ListMembership();
            Assert.IsTrue(list.Count >= 1);
        }

        [TestMethod()]
        public void ListByRoomIdAndPersonIdTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, false);
            Assert.IsNotNull(membership);

            var list = ListMembershipByPersonId(roomId, other.PersonId);
            Assert.IsNotNull(list);
            Assert.AreEqual(list[0].PersonId, other.PersonId);
            Assert.AreEqual(list[0].PersonEmail, other.Email);
            Assert.AreEqual(list[0].RoomId, roomId);
            Assert.IsFalse(list[0].IsModerator);
            Assert.IsFalse(list[0].IsMonitor);
        }

        [TestMethod()]
        public void ListByRoomIdAndEmailTest()
        {
            membership = CreateMembership(roomId, other.Email, null, false);
            Assert.IsNotNull(membership);

            var list = ListMembershipByEmail(roomId, other.Email);
            Assert.IsNotNull(list);
            Assert.AreEqual(list[0].PersonId, other.PersonId);
            Assert.AreEqual(list[0].PersonEmail, other.Email);
            Assert.AreEqual(list[0].RoomId, roomId);
            Assert.IsFalse(list[0].IsModerator);
            Assert.IsFalse(list[0].IsMonitor);
        }

        [TestMethod()]
        public void ListByRoomIdTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, false);
            Assert.IsNotNull(membership);

            var list = ListMembership(roomId);
            Assert.IsNotNull(list);

            var query  = list.Find(membership => membership.PersonId == other.PersonId);
            Assert.IsNotNull(query);
        }

        [TestMethod()]
        public void ListByRoomIdTest1()
        {
            membership = CreateMembership(roomId, other.Email, null, false);
            Assert.IsNotNull(membership);

            var list = ListMembership(roomId);
            Assert.IsNotNull(list);

            var query = list.Find(membership => membership.PersonEmail == other.Email);
            Assert.IsNotNull(query);
        }


        [TestMethod()]
        public void CreateByPersonIdTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, false);
            Validate(membership);

            Assert.AreEqual(roomId, membership.RoomId);
            Assert.AreEqual(other.Email, membership.PersonEmail);
            Assert.AreEqual(other.PersonId, membership.PersonId);
            Assert.IsFalse(membership.IsModerator);
            Assert.IsFalse(membership.IsMonitor);
        }

        [TestMethod()]
        public void CreateByPersonIdButInvalidRoomIdTest()
        {
            membership = CreateMembership("abc", null, other.PersonId, false);
            Assert.IsNull(membership);
        }

        [TestMethod()]
        public void CreateByInvalidPersonIdTest()
        {
            membership = CreateMembership(roomId, null, "abc", false);
            Assert.IsNull(membership);
        }

        [TestMethod()]
        public void CreateByPersonEmailTest()
        {
            membership = CreateMembership(roomId, other.Email, null, false);
            Validate(membership);

            Assert.AreEqual(roomId, membership.RoomId);
            Assert.AreEqual(other.Email, membership.PersonEmail);
            Assert.AreEqual(other.PersonId, membership.PersonId);
            Assert.IsFalse(membership.IsModerator);
            Assert.IsFalse(membership.IsMonitor);
        }

        [TestMethod()]
        public void CreateByPersonEmailButInvalidRoomIdTest()
        {
            membership = CreateMembership("abc", other.Email, null, false);
            Assert.IsNull(membership);
        }

        [TestMethod()]
        public void CreateByInvalidEmailTest()
        {
            membership = CreateMembership(roomId, "abc", null, false);
            Assert.IsNull(membership);
        }

        [TestMethod()]
        public void CreateModeratorPersonIdTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, true);
            Validate(membership);

            Assert.AreEqual(roomId, membership.RoomId);
            Assert.AreEqual(other.Email, membership.PersonEmail);
            Assert.AreEqual(other.PersonId, membership.PersonId);
            Assert.IsTrue(membership.IsModerator);
            Assert.IsFalse(membership.IsMonitor);
        }

        [TestMethod()]
        public void CreateModeratorPersonEmailTest()
        {
            membership = CreateMembership(roomId, other.Email, null, true);
            Validate(membership);

            Assert.AreEqual(roomId, membership.RoomId);
            Assert.AreEqual(other.Email, membership.PersonEmail);
            Assert.AreEqual(other.PersonId, membership.PersonId);
            Assert.IsTrue(membership.IsModerator);
            Assert.IsFalse(membership.IsMonitor);
        }

        [TestMethod()]
        public void GetTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, false);
            var query = GetMembership(membership.Id);
            Validate(query);
            Assert.AreEqual(membership.Id, query.Id);
            Assert.AreEqual(membership.PersonEmail, query.PersonEmail);
            Assert.AreEqual(membership.PersonId, query.PersonId);
        }

        [TestMethod()]
        public void GetInvalidIdTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, false);
            var query = GetMembership("abc");
            Assert.IsNull(query);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, false);
            var query = UpdateMembership(membership.Id, true);
            Validate(query);
            Assert.AreEqual(membership.PersonId, query.PersonId);
            Assert.IsTrue(query.IsModerator);
        }

        [TestMethod()]
        public void UpdateInvalidIdTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, false);
            var query = UpdateMembership("abc", true);
            Assert.IsNull(query);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, false);
            Validate(membership);
            Assert.IsTrue(DeleteMembership(membership.Id));

            Assert.IsNull(GetMembership(membership.Id));
        }

        [TestMethod()]
        public void DeleteInvalidIdTest()
        {
            membership = CreateMembership(roomId, null, other.PersonId, false);
            Validate(membership);
            Assert.IsFalse(DeleteMembership("abc"));

            Assert.IsNotNull(GetMembership(membership.Id));

        }

        private void Validate(Membership membership)
        {
            Assert.IsNotNull(membership);
            Assert.IsNotNull(membership.Id);
            Assert.IsNotNull(membership.PersonId);
            Assert.IsNotNull(membership.PersonEmail);
            Assert.IsNotNull(membership.RoomId);
            Assert.IsNotNull(membership.IsMonitor);
            Assert.IsNotNull(membership.IsModerator);
        }

        private Membership CreateMembership(string roomId, string email, string personId, bool isModerator)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Membership>();
            if (email != null)
            {
                memberships.CreateByPersonEmail(roomId, email, isModerator, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }
            else
            {
                memberships.CreateByPersonId(roomId, personId, isModerator, rsp =>
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

        private bool DeleteMembership(string membershipId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
  
            memberships.Delete(membershipId, rsp =>
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

        private List<Membership> ListMembership(int? max = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Membership>>();
            memberships.List(max, rsp=>
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

        private List<Membership> ListMembership(string roomId, int? max = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Membership>>();
            memberships.List(roomId, max, rsp =>
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

        private List<Membership> ListMembershipByPersonId(string roomId, string personId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Membership>>();
            memberships.ListByPersonId(roomId, personId, rsp =>
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

        private List<Membership> ListMembershipByEmail(string roomId, string email)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Membership>>();
            memberships.ListByPersonEmail(roomId, email, rsp =>
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

        private Membership GetMembership(string memshipId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Membership>();
            memberships.Get(memshipId, rsp =>
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

        private Membership UpdateMembership(string memshipId, bool isModerator)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Membership>();
            memberships.Update(memshipId, isModerator, rsp =>
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
    }
}