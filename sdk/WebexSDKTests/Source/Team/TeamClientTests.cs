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
    public class TeamClientTests
    {
        private WebexTestFixture fixture;
        private Webex webex;
        private TeamClient teams;
        private string teamName = "team_for_testing";
        private string updateTeamTitle = "team_for_testing_update";
        private string specialTitle = "@@@ &&&_%%%";
        private Team myTeamInfo;

        [TestInitialize]
        public void SetUp()
        {
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            //webex = fixture.webex;
            webex = fixture.CreateWebex();
            Assert.IsNotNull(webex);

            teams = webex.Teams;
            Assert.IsNotNull(teams);

            myTeamInfo = CreateTeam(teamName);
            Validate(myTeamInfo);
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
            var list = ListTeam();
            Assert.IsTrue(list.Count >= 1);
            Assert.IsNotNull(list.Find(team => team.Id == myTeamInfo.Id));
        }

        [TestMethod()]
        public void ListByMaxTest()
        {
            var list = ListTeam(1);
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod()]
        public void ListByInvalidIdTest()
        {
            var list = ListTeam(-1);
            Assert.IsNull(list);
        }


        [TestMethod()]
        public void CreateTest()
        {
            Assert.AreEqual(teamName, myTeamInfo.Name);
        }

        [TestMethod()]
        public void CreateNoNameTest()
        {
            var newTeam = CreateTeam("");
            Assert.IsNull(newTeam);
        }

        [TestMethod()]
        public void CreatebySpecialNameTest()
        {
            var newTeam = CreateTeam(specialTitle);
            Validate(newTeam);
            Assert.AreEqual(specialTitle, newTeam.Name);
            fixture.DeleteTeam(newTeam.Id);
        }

        [TestMethod()]
        public void GetTest()
        {
            var getResult = GetTeam(myTeamInfo.Id);
            Validate(getResult);
            Assert.AreEqual(myTeamInfo.Id, getResult.Id);
            Assert.AreEqual(myTeamInfo.Name, getResult.Name);
        }

        [TestMethod()]
        public void GetByInvalidIdTest()
        {
            var getResult = GetTeam("abc");
            Assert.IsNull(getResult);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            var newTeamInf = UpdateTeam(myTeamInfo.Id, updateTeamTitle);
            Assert.IsNotNull(newTeamInf);
            Assert.AreEqual(updateTeamTitle, newTeamInf.Name);

        }

        [TestMethod()]
        public void UpdateByInvalidIdTest()
        {
            var newTeamInf = UpdateTeam("abc", updateTeamTitle);
            Assert.IsNull(newTeamInf);
        }

        [TestMethod()]
        public void UpdateByEmptyNameTest()
        {
            var newTeamInf = UpdateTeam(myTeamInfo.Id, "");
            Assert.IsNotNull(newTeamInf);
            Assert.AreEqual("", newTeamInf.Name);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            Assert.IsTrue(fixture.DeleteTeam(myTeamInfo.Id));
            Assert.IsNull(GetTeam(myTeamInfo.Id));     
        }

        [TestMethod()]
        public void DeleteByInvalidIdTest()
        {
            Assert.IsFalse(fixture.DeleteTeam("abc"));
        }


        private void Validate(Team team)
        {
            Assert.IsNotNull(team);
            Assert.IsNotNull(team.Id);
            Assert.IsNotNull(team.Name);
            Assert.IsNotNull(team.Created);
        }

        private Team CreateTeam(string teamName)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Team>();
            webex.Teams.Create(teamName, rsp =>
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

        private Team GetTeam(string teamId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Team>();
            webex.Teams.Get(teamId, rsp =>
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

        private Team UpdateTeam(string teamId, string name)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Team>();
            webex.Teams.Update(teamId, name, rsp =>
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

        private List<Team> ListTeam(int? max = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Team>>();
            webex.Teams.List(max, rsp =>
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


    }
}