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
    public class TeamMembershipClientTests
    {
        private WebexTestFixture fixture;
        private Webex webex;
        private TeamMembershipClient teamMemberships;
        private Team myTeamInfo;

        [TestInitialize]
        public void SetUp()
        {
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            //webex = fixture.webex;
            webex = fixture.CreateWebex();
            Assert.IsNotNull(webex);

            teamMemberships = webex.TeamMemberships;
            Assert.IsNotNull(teamMemberships);

            myTeamInfo = fixture.CreateTeam("test team");
            Assert.IsNotNull(myTeamInfo);
        }


        [TestCleanup]
        public void TearDown()
        {
            if (myTeamInfo != null)
            {
                fixture.DeleteTeam(myTeamInfo.Id);
            }
        }

        [TestMethod()]
        public void ListTest()
        {
            var userOne = fixture.CreatUser();
            var userTwo = fixture.CreatUser();

            var memberOne = CreateTeamMemberShip(myTeamInfo.Id, null, userOne.Email, null);
            Validate(memberOne);
            var memberTwo = CreateTeamMemberShip(myTeamInfo.Id, null, userTwo.Email, null);
            Validate(memberTwo);
            var list = ListTeamMembership(myTeamInfo.Id);
            Assert.AreEqual(3, list.Count);
            var qurey = list.Find(teamMembership => teamMembership.PersonEmail == fixture.selfUser.Email);
            Validate(qurey);
            qurey = list.Find(teamMembership => teamMembership.PersonEmail == userOne.Email);
            Validate(qurey);
            qurey = list.Find(teamMembership => teamMembership.PersonId == userTwo.PersonId);
            Validate(qurey);
        }

        [TestMethod()]
        public void ListByMaxTest()
        {
            var userOne = fixture.CreatUser();
            var userTwo = fixture.CreatUser();

            var memberOne = CreateTeamMemberShip(myTeamInfo.Id, null, userOne.Email, null);
            Validate(memberOne);
            var memberTwo = CreateTeamMemberShip(myTeamInfo.Id, null, userTwo.Email, null);
            Validate(memberTwo);
            var list = ListTeamMembership(myTeamInfo.Id, 2);
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod()]
        public void ListByInvalidMaxTest()
        {
            var userOne = fixture.CreatUser();
            var userTwo = fixture.CreatUser();

            var memberOne = CreateTeamMemberShip(myTeamInfo.Id, null, userOne.Email, null);
            Validate(memberOne);
            var memberTwo = CreateTeamMemberShip(myTeamInfo.Id, null, userTwo.Email, null);
            Validate(memberTwo);
            var list = ListTeamMembership(myTeamInfo.Id, -1);
            Assert.AreEqual(3, list.Count);
        }


        [TestMethod()]
        public void CreateByIdTest()
        {
            var user = fixture.CreatUser();
            Assert.IsNotNull(user);
            var member = CreateTeamMemberShip(myTeamInfo.Id, user.PersonId, null, null);
            Validate(member);
            Assert.AreEqual(user.PersonId, member.PersonId);
        }

        [TestMethod()]
        public void CreateByIdAndIsModeratorTest()
        {
            var user = fixture.CreatUser();
            Assert.IsNotNull(user);
            var member = CreateTeamMemberShip(myTeamInfo.Id, user.PersonId, null, true);
            Validate(member);
            Assert.AreEqual(user.PersonId, member.PersonId);
            Assert.IsTrue(member.IsModerator);
        }

        [TestMethod()]
        public void CreateByEmailTest()
        {
            var user = fixture.CreatUser();
            Assert.IsNotNull(user);
            var member = CreateTeamMemberShip(myTeamInfo.Id, null, user.Email, null);
            Validate(member);
            Assert.AreEqual(user.Email, member.PersonEmail);
        }

        [TestMethod()]
        public void CreateByEmailAndIsModeratorTest()
        {
            var user = fixture.CreatUser();
            Assert.IsNotNull(user);
            var member = CreateTeamMemberShip(myTeamInfo.Id, null, user.Email, true);
            Validate(member);
            Assert.AreEqual(user.Email, member.PersonEmail);
            Assert.IsTrue(member.IsModerator);
        }


        [TestMethod()]
        public void CreateByInvalildTeamIdTest()
        {
            var user = fixture.CreatUser();
            Assert.IsNotNull(user);
            var member = CreateTeamMemberShip("abc", user.PersonId, null, null);
            Assert.IsNull(member);
        }

        [TestMethod()]
        public void CreateByInvalildPersonIdTest()
        {
            var user = fixture.CreatUser();
            Assert.IsNotNull(user);
            var member = CreateTeamMemberShip(myTeamInfo.Id, "abc", null, null);
            Assert.IsNull(member);
        }

        [TestMethod()]
        public void CreateByInvalildEmailTest()
        {
            var user = fixture.CreatUser();
            Assert.IsNotNull(user);
            var member = CreateTeamMemberShip(myTeamInfo.Id, null, "abc", null);
            Assert.IsNull(member);
        }


        [TestMethod()]
        public void GetTest()
        {
            var userOne = fixture.CreatUser();

            var memberOne = CreateTeamMemberShip(myTeamInfo.Id, null, userOne.Email, null);
            Validate(memberOne);

            var getResult = GetTeamMembership(memberOne.Id);
            Assert.AreEqual(memberOne.Id, getResult.Id);
            Assert.AreEqual(memberOne.PersonId, getResult.PersonId);
        }

        [TestMethod()]
        public void GetByInvalidIdTest()
        {
            var getResult = GetTeamMembership("abc");
            Assert.IsNull(getResult);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            var userOne = fixture.CreatUser();

            var memberOne = CreateTeamMemberShip(myTeamInfo.Id, null, userOne.Email, false);
            Validate(memberOne);
            Assert.IsFalse(memberOne.IsModerator);

            var updateResult = UpdateTeamMembership(memberOne.Id, true);
            Assert.IsTrue(updateResult.IsModerator);

            var getResult = GetTeamMembership(memberOne.Id);
            Assert.IsTrue(getResult.IsModerator);

            updateResult = UpdateTeamMembership(memberOne.Id, false);
            Assert.IsFalse(updateResult.IsModerator);

            getResult = GetTeamMembership(memberOne.Id);
            Assert.IsFalse(getResult.IsModerator);

        }

        [TestMethod()]
        public void UpdateByInvalidIdTest()
        {
            var updateResult = UpdateTeamMembership("abc", true);
            Assert.IsNull(updateResult);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            var userOne = fixture.CreatUser();

            var memberOne = CreateTeamMemberShip(myTeamInfo.Id, null, userOne.Email, false);
            Validate(memberOne);

            Assert.IsTrue(DeleteTeamMembership(memberOne.Id));
            Assert.IsNull(GetTeamMembership(memberOne.Id));
            
        }

        [TestMethod()]
        public void DeleteLastOneTest()
        {
            var list = ListTeamMembership(myTeamInfo.Id);
            Assert.AreEqual(1, list.Count);
            var findResult = list.Find(member => member.PersonId == fixture.selfUser.PersonId);
            Validate(findResult);
            Assert.IsTrue(findResult.IsModerator);

            Assert.IsTrue(DeleteTeamMembership(findResult.Id));

            Assert.IsNull(ListTeamMembership(myTeamInfo.Id));

        }

        private void Validate(TeamMembership teamMembership)
        {
            Assert.IsNotNull(teamMembership);
            Assert.IsNotNull(teamMembership.Id);
            Assert.IsNotNull(teamMembership.PersonId);
            Assert.IsNotNull(teamMembership.PersonEmail);
            Assert.IsNotNull(teamMembership.TeamId);
            Assert.IsNotNull(teamMembership.PersonDisplayName);
            Assert.IsNotNull(teamMembership.IsModerator);
        }

        private TeamMembership CreateTeamMemberShip(string teamId, string personId, string email, bool? isModerator)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<TeamMembership>();
            if (email != null)
            {
                teamMemberships.CreateByEmail(teamId, email, isModerator, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }
            else
            {
                teamMemberships.CreateById(teamId, personId, isModerator, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }


            if (!completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess)
            {
                return response.Data;
            }

            return null;
        }

        private List<TeamMembership> ListTeamMembership(string teamId, int? max = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<TeamMembership>>();
            teamMemberships.List(teamId, max, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess)
            {
                return response.Data;
            }

            return null;

        }

        private TeamMembership GetTeamMembership(string membershipId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<TeamMembership>();
            teamMemberships.Get(membershipId, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess)
            {
                return response.Data;
            }

            return null;

        }

        private TeamMembership UpdateTeamMembership(string membershipId, bool? isModerator)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<TeamMembership>();
            teamMemberships.Update(membershipId, isModerator, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess)
            {
                return response.Data;
            }

            return null;

        }


        private bool DeleteTeamMembership(string membershipId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
            teamMemberships.Delete(membershipId, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                return false;
            }

            if (response.IsSuccess)
            {
                return true;
            }

            return false;

        }
    }

    
}