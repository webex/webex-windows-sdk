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
    public class RoomClientTests
    {
        private WebexTestFixture fixture;
        private Webex webex;
        private RoomClient room;
        private string roomTitle = "room_for_testing";
        private string updateRoomTitle = "room_for_testing_update";
        private string specialTitle = "@@@ &&&_%%%";
        private Room myRoomInfo;

        [TestInitialize]
        public void SetUp()
        {
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            //webex = fixture.webex;
            webex = fixture.CreateWebex();
            Assert.IsNotNull(webex);

            room = webex.Rooms;
            Assert.IsNotNull(room);
        }


        [TestCleanup]
        public void TearDown()
        {
            if (myRoomInfo != null)
            {
                if(DeleteRoom(myRoomInfo.Id) != true)
                {
                    Console.WriteLine("fail to delete room[{0}]", myRoomInfo.Id);
                }
            }
        }

        [TestMethod()]
        public void CreateByRoomTitleTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);
            Assert.AreEqual(roomTitle, myRoomInfo.Title);
            Assert.AreEqual(RoomType.Group, myRoomInfo.Type);
        }

        [TestMethod()]
        public void CreateByEmptyRoomTitleTest()
        {
            myRoomInfo = CreateRoom("");
            Assert.IsNotNull(myRoomInfo);
            Assert.IsNotNull(myRoomInfo.Id);
            Assert.IsNull(myRoomInfo.Title);
        }

        [TestMethod()]
        public void CreateBySpecialRoomTitleTest()
        {
            myRoomInfo = CreateRoom(specialTitle);
            Validate(myRoomInfo);
            Assert.AreEqual(specialTitle, myRoomInfo.Title);
        }

        [TestMethod()]
        public void CreateByRoomTitleAndTeamIdTest()
        {
            var myTeam = fixture.CreateTeam("test team");
            var myRoom = CreateRoom(roomTitle, myTeam.Id);
            Validate(myRoom);
        }

        [TestMethod()]
        public void GetTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var getRoomInfo = GetRoom(myRoomInfo.Id);
            Validate(getRoomInfo);

            Assert.AreEqual(myRoomInfo.Id, getRoomInfo.Id);
            Assert.AreEqual(myRoomInfo.Title, getRoomInfo.Title);
        }

        [TestMethod()]
        public void GetByInvalidIdTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var getRoomInfo = GetRoom("abc");
            Assert.IsNull(getRoomInfo);
        }

        [TestMethod()]
        public void GetByEmptyIdTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var getRoomInfo = GetRoom("");
            Assert.IsNotNull(getRoomInfo);
            Assert.IsNull(getRoomInfo.Id);
            Assert.IsNull(getRoomInfo.Title);
        }

        [TestMethod()]
        public void UpdateByRooIdAndTitleTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var newRoom = UpdateRoom(myRoomInfo.Id, updateRoomTitle);
            Validate(newRoom);

            Assert.AreEqual(updateRoomTitle, newRoom.Title);
        }

        [TestMethod()]
        public void UpdateByRooIdAndSpecialTitleTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var newRoom = UpdateRoom(myRoomInfo.Id, specialTitle);
            Validate(newRoom);

            Assert.AreEqual(specialTitle, newRoom.Title);
        }


        [TestMethod()]
        public void UpdateByInvalidRoomIdTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var newRoom = UpdateRoom("abc", roomTitle);
            Assert.IsNull(newRoom);
        }


        [TestMethod()]
        public void DeleteTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            Assert.IsTrue(DeleteRoom(myRoomInfo.Id));

            Assert.IsNull(GetRoom(myRoomInfo.Id));
       
        }

        [TestMethod()]
        public void DeleteByInvalidIdTest()
        {
            Assert.IsFalse(DeleteRoom("abc"));
        }

        [TestMethod()]
        public void ListTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var lstRoom = ListRooms();
            Assert.IsNotNull(lstRoom);
            Validate(lstRoom[0]);
            Assert.AreEqual(myRoomInfo.Title, lstRoom[0].Title);
            Assert.AreEqual(myRoomInfo.Id, lstRoom[0].Id);

        }

        [TestMethod()]
        public void ListByMaxCountTest()
        {
            int roomCount = 11;
            List<Room> lstRooms = new List<Room>();
            for (int i = 0; i < roomCount; i++)
            {
                lstRooms.Add(CreateRoom(string.Format("room#{0}", i)));
            }

            var lstRoom = ListRooms(null, 10, null);

            for (int i = 0; i < roomCount; i++)
            {
                DeleteRoom(lstRooms[i].Id);
            }

            Assert.AreEqual(10, lstRoom.Count);
        }

        [TestMethod()]
        public void ListByInvalidMaxCountTest()
        {
            int roomCount = 11;
            List<Room> lstRooms = new List<Room>();
            for (int i = 0; i < roomCount; i++)
            {
                lstRooms.Add(CreateRoom(string.Format("room#{0}", i)));
            }

            var lstRoom = ListRooms(null, -1, null);

            for (int i = 0; i < roomCount; i++)
            {
                DeleteRoom(lstRooms[i].Id);
            }
            Assert.IsNull(lstRoom);
        }

        [TestMethod()]
        public void ListByRoomTypeTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var lstRoom = ListRooms(null, null, RoomType.Group);
            Assert.IsNotNull(lstRoom);

            Validate(lstRoom[0]);
            Assert.AreEqual(myRoomInfo.Title, lstRoom[0].Title);
            Assert.AreEqual(myRoomInfo.Id, lstRoom[0].Id);
        }

        [TestMethod()]
        public void ListByTeamIdTest()
        {
            var myTeam = fixture.CreateTeam("test team");
            Assert.IsNotNull(myTeam);
            var room1 = CreateRoom("room1", myTeam.Id);
            var room2 = CreateRoom("room2", myTeam.Id);

            var rooms = ListRooms(myTeam.Id);
            Assert.IsNotNull(rooms);
            Assert.IsNotNull(rooms.Find(room => room.Id == room1.Id));
            Assert.IsNotNull(rooms.Find(room => room.Id == room2.Id));

            DeleteRoom(room1.Id);
            DeleteRoom(room2.Id);
            fixture.DeleteTeam(myTeam.Id);
        }

        [TestMethod()]
        public void ListBySortByLastActivityTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var lstRoom = ListRooms(null, null, null, RoomSortType.ByLastActivity);
            Assert.IsNotNull(lstRoom);

            Validate(lstRoom[0]);
            Assert.AreEqual(myRoomInfo.Title, lstRoom[0].Title);
            Assert.AreEqual(myRoomInfo.Id, lstRoom[0].Id);
        }
        [TestMethod()]
        public void ListBySortByIdTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var lstRoom = ListRooms(null, null, null, RoomSortType.ById);
            Assert.IsNotNull(lstRoom);
            Assert.IsNotNull(lstRoom.Find(item => item.Id == myRoomInfo.Id));
        }
        [TestMethod()]
        public void ListBySortByCreatedTest()
        {
            myRoomInfo = CreateRoom(roomTitle);
            Validate(myRoomInfo);

            var lstRoom = ListRooms(null, null, null, RoomSortType.ByCreated);
            Assert.IsNotNull(lstRoom);

            Validate(lstRoom[0]);
            Assert.AreEqual(myRoomInfo.Title, lstRoom[0].Title);
            Assert.AreEqual(myRoomInfo.Id, lstRoom[0].Id);
        }

        private void Validate(Room room)
        {
            Assert.IsNotNull(room);
            Assert.IsNotNull(room.Id);
            Assert.IsNotNull(room.Title);
            Assert.IsNotNull(room.Type);
            Assert.IsNotNull(room.IsLocked);
            Assert.IsNotNull(room.LastActivity);
            Assert.IsNotNull(room.Created);
        }

        private Room CreateRoom(string title, string teamId = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Room>();
            room.Create(title, teamId, rsp =>
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

        private Room UpdateRoom(string roomId, string title)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Room>();
            room.Update(roomId, title, rsp =>
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

        private Room GetRoom(string roomId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Room>();
            room.Get(roomId, rsp =>
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


        private List<Room> ListRooms(string teamId = null, int? max = null, RoomType? roomType = null, RoomSortType? sortby = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Room>>();
            room.List(teamId, max, roomType, sortby, rsp =>
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

        private bool DeleteRoom(string roomId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
            room.Delete(roomId, rsp =>
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