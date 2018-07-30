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
    public class PersonClientTests
    {
        private WebexTestFixture fixture;
        private Webex webex;
        private PersonClient person;
        private TestUser other;

        [TestInitialize]
        public void SetUp()
        {
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            //webex = fixture.webex;
            webex = fixture.CreateWebex();
            Assert.IsNotNull(webex);

            person = webex.People;
            Assert.IsNotNull(person);

            other = fixture.CreatUser();
            Assert.IsNotNull(other);
        }
        

        [TestCleanup]
        public void TearDown()
        {
            
        }

        [TestMethod()]
        public void ListByEmailTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(other.Email, null, null, rsp=>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(response.IsSuccess);
            List<Person> persons = response.Data;
            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual(other.Email, persons[0].Emails[0]);
            Assert.AreEqual(other.Name, persons[0].DisplayName);
            Assert.AreEqual(other.PersonId, persons[0].Id);
        }

        [TestMethod()]
        public void ListByDisplayNameTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(null, other.Name, null, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(response.IsSuccess);
            List<Person> persons = response.Data;
            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual(other.Email, persons[0].Emails[0]);
            Assert.AreEqual(other.Name, persons[0].DisplayName);
            Assert.AreEqual(other.PersonId, persons[0].Id);
        }

        [TestMethod()]
        public void ListByWrongDisplayNameTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(null, "notexist", null, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(response.IsSuccess);
            List<Person> persons = response.Data;
            Assert.AreEqual(0, persons.Count);
        }

        [TestMethod()]
        public void ListByEmailAndDisplayNameTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(other.Email, other.Name, null, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(response.IsSuccess);
            List<Person> persons = response.Data;
            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual(other.Email, persons[0].Emails[0]);
            Assert.AreEqual(other.Name, persons[0].DisplayName);
            Assert.AreEqual(other.PersonId, persons[0].Id);
        }

        [TestMethod()]
        public void ListByEmailAndDisplayNameTestAndCount()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(other.Email, other.Name, 10, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(response.IsSuccess);
            List<Person> persons = response.Data;
            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual(other.Email, persons[0].Emails[0]);
            Assert.AreEqual(other.Name, persons[0].DisplayName);
            Assert.AreEqual(other.PersonId, persons[0].Id);
        }

        [TestMethod()]
        public void ListByEmailAndCount()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(other.Email, null, 10, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(response.IsSuccess);
            List<Person> persons = response.Data;
            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual(other.Email, persons[0].Emails[0]);
            Assert.AreEqual(other.Name, persons[0].DisplayName);
            Assert.AreEqual(other.PersonId, persons[0].Id);
        }

        [TestMethod()]
        public void ListByDisplayNameAndCount()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(null, other.Name, 10, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(response.IsSuccess);
            List<Person> persons = response.Data;
            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual(other.Email, persons[0].Emails[0]);
            Assert.AreEqual(other.Name, persons[0].DisplayName);
            Assert.AreEqual(other.PersonId, persons[0].Id);
        }


        [TestMethod()]
        public void ListByCount()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(null, null, 10, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
        }

        [TestMethod()]
        public void ListByNothing()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(null, null, null, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
        }

        [TestMethod()]
        public void ListByEmailAndDisplayNameTestAndInvalidCount()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(other.Email, other.Name, -1, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
        }

        [TestMethod()]
        public void ListByEmailAndInvalidCount()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(other.Email, null, -1, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
        }

        [TestMethod()]
        public void ListByDisplayNameAndInvalidCount()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(null, other.Name, -1, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
        }


        [TestMethod()]
        public void ListByInvalidCount()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Person>>();
            person.List(null, null, -1, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
        }

        [TestMethod()]
        public void GetTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Person>();
            person.Get(other.PersonId, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(response.IsSuccess);
            Person people = response.Data;
            Assert.AreEqual(other.Email, people.Emails[0]);
            Assert.AreEqual(other.Name, people.DisplayName);
            Assert.AreEqual(other.PersonId, people.Id);
        }

        [TestMethod()]
        public void GetbyWrongIdTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Person>();
            person.Get("abcd", rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
        }


        [TestMethod()]
        public void GetByNullIdTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Person>();
            person.Get(null, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
        }


        [TestMethod()]
        public void GetWithoutIdTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Person>();
            person.Get("", rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsFalse(response.IsSuccess);
        }
        [TestMethod()]
        public void GetMeTest()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Person>();
            person.GetMe(rsp=>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Assert.Fail();
                return;
            }

            Assert.IsTrue(response.IsSuccess);
            Person me = response.Data;
            Assert.AreEqual(fixture.selfUser.Email, me.Emails[0]);
            Assert.AreEqual(fixture.selfUser.Name, me.DisplayName);
            Assert.AreEqual(fixture.selfUser.PersonId, me.Id);
        }
    }
}