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
    public class SpaceClientTests
    {
        private WebexTestFixture fixture;
        private Webex webex;
        private SpaceClient space;
        private string spaceTitle = "space_for_testing";
        private string updateSpaceTitle = "space_for_testing_update";
        private string specialTitle = "@@@ &&&_%%%";
        private Space mySpaceInfo;

        [TestInitialize]
        public void SetUp()
        {
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            //webex = fixture.webex;
            webex = fixture.CreateWebex();
            Assert.IsNotNull(webex);

            space = webex.Spaces;
            Assert.IsNotNull(space);
        }


        [TestCleanup]
        public void TearDown()
        {
            if (mySpaceInfo != null)
            {
                if(DeleteSpace(mySpaceInfo.Id) != true)
                {
                    Console.WriteLine("fail to delete space[{0}]", mySpaceInfo.Id);
                }
            }
        }

        [TestMethod()]
        public void CreateBySpaceTitleTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);
            Assert.AreEqual(spaceTitle, mySpaceInfo.Title);
            Assert.AreEqual(SpaceType.Group, mySpaceInfo.Type);
        }

        [TestMethod()]
        public void CreateByEmptySpaceTitleTest()
        {
            mySpaceInfo = CreateSpace("");
            Assert.IsNotNull(mySpaceInfo);
            Assert.IsNotNull(mySpaceInfo.Id);
            Assert.IsNull(mySpaceInfo.Title);
        }

        [TestMethod()]
        public void CreateBySpecialSpaceTitleTest()
        {
            mySpaceInfo = CreateSpace(specialTitle);
            Validate(mySpaceInfo);
            Assert.AreEqual(specialTitle, mySpaceInfo.Title);
        }

        [TestMethod()]
        public void CreateBySpaceTitleAndTeamIdTest()
        {
            var myTeam = fixture.CreateTeam("test team");
            var mySpace = CreateSpace(spaceTitle, myTeam.Id);
            Validate(mySpace);
        }

        [TestMethod()]
        public void GetTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var getSpaceInfo = GetSpace(mySpaceInfo.Id);
            Validate(getSpaceInfo);

            Assert.AreEqual(mySpaceInfo.Id, getSpaceInfo.Id);
            Assert.AreEqual(mySpaceInfo.Title, getSpaceInfo.Title);
        }

        [TestMethod()]
        public void GetByInvalidIdTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var getSpaceInfo = GetSpace("abc");
            Assert.IsNull(getSpaceInfo);
        }

        [TestMethod()]
        public void GetByEmptyIdTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var getSpaceInfo = GetSpace("");
            Assert.IsNotNull(getSpaceInfo);
            Assert.IsNull(getSpaceInfo.Id);
            Assert.IsNull(getSpaceInfo.Title);
        }

        [TestMethod()]
        public void UpdateBySpaceIdAndTitleTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var newSpace = UpdateSpace(mySpaceInfo.Id, updateSpaceTitle);
            Validate(newSpace);

            Assert.AreEqual(updateSpaceTitle, newSpace.Title);
        }

        [TestMethod()]
        public void UpdateBySpaceIdAndSpecialTitleTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var newSpace = UpdateSpace(mySpaceInfo.Id, specialTitle);
            Validate(newSpace);

            Assert.AreEqual(specialTitle, newSpace.Title);
        }


        [TestMethod()]
        public void UpdateByInvalidSpaceIdTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var newSpace = UpdateSpace("abc", spaceTitle);
            Assert.IsNull(newSpace);
        }


        [TestMethod()]
        public void DeleteTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            Assert.IsTrue(DeleteSpace(mySpaceInfo.Id));

            Assert.IsNull(GetSpace(mySpaceInfo.Id));
       
        }

        [TestMethod()]
        public void DeleteByInvalidIdTest()
        {
            Assert.IsFalse(DeleteSpace("abc"));
        }

        [TestMethod()]
        public void ListTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var lstSpace = ListSpaces();
            Assert.IsNotNull(lstSpace);
            Validate(lstSpace[0]);
            Assert.AreEqual(mySpaceInfo.Title, lstSpace[0].Title);
            Assert.AreEqual(mySpaceInfo.Id, lstSpace[0].Id);

        }

        [TestMethod()]
        public void ListByMaxCountTest()
        {
            int spaceCount = 11;
            List<Space> lstSpaces = new List<Space>();
            for (int i = 0; i < spaceCount; i++)
            {
                lstSpaces.Add(CreateSpace(string.Format("space#{0}", i)));
            }

            var lstSpace = ListSpaces(null, 10, null);

            for (int i = 0; i < spaceCount; i++)
            {
                DeleteSpace(lstSpaces[i].Id);
            }

            Assert.AreEqual(10, lstSpace.Count);
        }

        [TestMethod()]
        public void ListByInvalidMaxCountTest()
        {
            int spaceCount = 11;
            List<Space> lstSpaces = new List<Space>();
            for (int i = 0; i < spaceCount; i++)
            {
                lstSpaces.Add(CreateSpace(string.Format("space#{0}", i)));
            }

            var lstSpace = ListSpaces(null, -1, null);

            for (int i = 0; i < spaceCount; i++)
            {
                DeleteSpace(lstSpaces[i].Id);
            }
            Assert.IsNull(lstSpace);
        }

        [TestMethod()]
        public void ListBySpaceTypeTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var lstSpace = ListSpaces(null, null, SpaceType.Group);
            Assert.IsNotNull(lstSpace);

            Validate(lstSpace[0]);
            Assert.AreEqual(mySpaceInfo.Title, lstSpace[0].Title);
            Assert.AreEqual(mySpaceInfo.Id, lstSpace[0].Id);
        }

        [TestMethod()]
        public void ListByTeamIdTest()
        {
            var myTeam = fixture.CreateTeam("test team");
            Assert.IsNotNull(myTeam);
            var space1 = CreateSpace("space1", myTeam.Id);
            var space2 = CreateSpace("space2", myTeam.Id);

            var spaces = ListSpaces(myTeam.Id);
            Assert.IsNotNull(spaces);
            Assert.IsNotNull(spaces.Find(space => space.Id == space1.Id));
            Assert.IsNotNull(spaces.Find(space => space.Id == space2.Id));

            DeleteSpace(space1.Id);
            DeleteSpace(space2.Id);
            fixture.DeleteTeam(myTeam.Id);
        }

        [TestMethod()]
        public void ListBySortByLastActivityTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var lstSpace = ListSpaces(null, null, null, SpaceSortType.ByLastActivity);
            Assert.IsNotNull(lstSpace);

            Validate(lstSpace[0]);
            Assert.AreEqual(mySpaceInfo.Title, lstSpace[0].Title);
            Assert.AreEqual(mySpaceInfo.Id, lstSpace[0].Id);
        }
        [TestMethod()]
        public void ListBySortByIdTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var lstSpace = ListSpaces(null, null, null, SpaceSortType.ById);
            Assert.IsNotNull(lstSpace);
            Assert.IsNotNull(lstSpace.Find(item => item.Id == mySpaceInfo.Id));
        }
        [TestMethod()]
        public void ListBySortByCreatedTest()
        {
            mySpaceInfo = CreateSpace(spaceTitle);
            Validate(mySpaceInfo);

            var lstSpace = ListSpaces(null, null, null, SpaceSortType.ByCreated);
            Assert.IsNotNull(lstSpace);

            Validate(lstSpace[0]);
            Assert.AreEqual(mySpaceInfo.Title, lstSpace[0].Title);
            Assert.AreEqual(mySpaceInfo.Id, lstSpace[0].Id);
        }

        private void Validate(Space space)
        {
            Assert.IsNotNull(space);
            Assert.IsNotNull(space.Id);
            Assert.IsNotNull(space.Title);
            Assert.IsNotNull(space.Type);
            Assert.IsNotNull(space.IsLocked);
            Assert.IsNotNull(space.LastActivity);
            Assert.IsNotNull(space.Created);
        }

        private Space CreateSpace(string title, string teamId = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Space>();
            space.Create(title, teamId, rsp =>
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

        private Space UpdateSpace(string spaceId, string title)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Space>();
            space.Update(spaceId, title, rsp =>
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

        private Space GetSpace(string spaceId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Space>();
            space.Get(spaceId, rsp =>
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


        private List<Space> ListSpaces(string teamId = null, int? max = null, SpaceType? spaceType = null, SpaceSortType? sortby = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Space>>();
            space.List(teamId, max, spaceType, sortby, rsp =>
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

        private bool DeleteSpace(string spaceId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
            space.Delete(spaceId, rsp =>
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