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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace WebexSDK.Tests
{


    [TestClass()]
    public class PhoneTests
    {
        private static WebexTestFixture fixture;
        private static Webex webex;
        private static Person self;
        private static Phone phone;
        private static Space mySpace;
        private Call currentCall;
        private CallData callData;

        //private static Process testFixtureAppProcess = null;
        private readonly string calleeAddress = ConfigurationManager.AppSettings["TestFixtureAppAddress01"] ?? "";
        private static readonly string testFixtureApp = "TestFixtureApp";



        private static bool StartTestFixtureAppProcess()
        {
            //on Jenkins, need manual start TestFixtureApp. on local, you can commit this line to auto start and close the TestFixtureApp.
            return true;
            //Console.WriteLine("start testFixtureAppProcess");
            //testFixtureAppProcess = new Process();
            //testFixtureAppProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory + "\\testApp";
            //testFixtureAppProcess.StartInfo.FileName = "testFixtureApp.exe";
            //bool fileExist = File.Exists(testFixtureAppProcess.StartInfo.WorkingDirectory + testFixtureAppProcess.StartInfo.FileName);
            //if (!fileExist)
            //{
            //    testFixtureAppProcess.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            //}
            //return testFixtureAppProcess.Start();
        }

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            Console.WriteLine("ClassSetup");
            fixture = WebexTestFixture.Instance;
            Assert.IsNotNull(fixture);

            webex = fixture.CreateWebexbyJwt();
            Assert.IsNotNull(webex);

            self = GetMe();

            phone = webex.Phone;
            Assert.IsNotNull(phone);

            phone.DisableVideoCodecActivation(true);

            Assert.IsTrue(RegisterPhone());

            mySpace = CreateSpace("my test space");
            Assert.IsNotNull(mySpace);

            // start testFixtureApp process
            //Assert.IsTrue(StartTestFixtureAppProcess());
            //Thread.Sleep(30000);

            Thread.Sleep(10000);
        }

        [ClassCleanup]
        public static void ClassTearDown()
        {
            Console.WriteLine("ClassTearDown");
            if (mySpace != null)
            {
                DeleteSpace(mySpace.Id);
                mySpace = null;
            }

            //if (testFixtureAppProcess != null && !testFixtureAppProcess.HasExited)
            //{
            //    Console.WriteLine("close testFixtureAppProcess");
            //    testFixtureAppProcess.Kill();
            //    testFixtureAppProcess = null;
            //}

            fixture = null;
            webex = null;
            phone = null;
            mySpace = null;
        }

        [TestInitialize]
        public void SetUp()
        {
            Console.WriteLine("set up");
            currentCall = null;
            callData = new CallData();
            Assert.IsTrue(RegisterPhone());
        }


        [TestCleanup]
        public void TearDown()
        {
            Console.WriteLine("tear down");
            //if (testFixtureAppProcess != null && testFixtureAppProcess.HasExited)
            //{
            //    Console.WriteLine("Error: testFixtureApp has exited.");
            //    Assert.IsTrue(StartTestFixtureAppProcess());
            //    Thread.Sleep(10000);
            //}

            if (currentCall != null && currentCall.Status != CallStatus.Disconnected)
            {
                Console.WriteLine($"Error: call is not disconnected in the end. CallStatus:{currentCall.Status.ToString()} CallDirection:{currentCall.Direction.ToString()}");
                HangupCall(currentCall, 2000);
                MessageHelper.RunDispatcherLoop();
            }
            Thread.Sleep(10000);
        }

        [TestMethod()]
        public void RegisterTwiceThenBothSucceedTest()
        {
            Assert.IsTrue(RegisterPhone());
        }

        [TestMethod()]
        public void DeregisterTest()
        {
            Assert.IsTrue(DeregisterPhone());
        }

        [TestMethod()]
        public void DeregisterThenBothSucceedTest()
        {
            Assert.IsTrue(DeregisterPhone());
            Assert.IsTrue(DeregisterPhone());
        }

        [TestMethod()]
        public void DialAudioVideoCallAndHangUpTest()
        {
            var callee = fixture.CreatUser();
            Assert.IsNotNull(callee);

            currentCall = null;
            bool result = DialCall(callee.Email, MediaOption.AudioVideo(), ref currentCall);
            Assert.IsTrue(result);
            if (currentCall != null)
            {
                HangupCall(currentCall);
                MessageHelper.RunDispatcherLoop();
            }

            Assert.IsNotNull(currentCall);
            Assert.IsTrue(currentCall.Status >= CallStatus.Initiated);
            Assert.AreEqual(Call.CallDirection.Outgoing, currentCall.Direction);
        }

        [TestMethod()]
        public void DialAudioVideoShareCallAndHangUpTest()
        {
            var callee = fixture.CreatUser();
            Assert.IsNotNull(callee);

            currentCall = null;
            bool result = DialCall(callee.Email, MediaOption.AudioVideoShare(), ref currentCall);
            Assert.IsTrue(result);
            if (currentCall != null)
            {
                HangupCall(currentCall);
                MessageHelper.RunDispatcherLoop();
            }

            Assert.IsNotNull(currentCall);
            Assert.IsTrue(currentCall.Status >= CallStatus.Initiated);
            Assert.AreEqual(Call.CallDirection.Outgoing, currentCall.Direction);
            Assert.IsTrue(callData.ReleaseReason is LocalCancel);
        }

        [TestMethod()]
        public void DialAudioCallAndHangUpTest()
        {
            var callee = fixture.CreatUser();
            Assert.IsNotNull(callee);

            currentCall = null;
            bool result = DialCall(callee.Email, MediaOption.AudioOnly(), ref currentCall);
            Assert.IsTrue(result);
            if (currentCall != null)
            {
                HangupCall(currentCall);
                MessageHelper.RunDispatcherLoop();
            }

            Assert.IsNotNull(currentCall);
            Assert.IsTrue(currentCall.Status >= CallStatus.Initiated);
            Assert.AreEqual(Call.CallDirection.Outgoing, currentCall.Direction);
        }

        [TestMethod()]
        public void DialWithHydraPersonIdAndHangUpTest()
        {
            var callee = fixture.CreatUser();
            Assert.IsNotNull(callee);

            currentCall = null;
            bool result = DialCall(callee.PersonId, MediaOption.AudioVideoShare(), ref currentCall);
            Assert.IsTrue(result);
            if (currentCall != null)
            {
                HangupCall(currentCall);
                MessageHelper.RunDispatcherLoop();
            }

            Assert.IsNotNull(currentCall);
            Assert.IsTrue(currentCall.Status >= CallStatus.Initiated);
            Assert.AreEqual(Call.CallDirection.Outgoing, currentCall.Direction);
        }

        [TestMethod()]
        public void DialWithJwtUserAndHangUpTest()
        {
            currentCall = null;
            bool result = DialCall(calleeAddress, MediaOption.AudioVideoShare(), ref currentCall);
            Assert.IsTrue(result);
            if (currentCall != null)
            {
                HangupCall(currentCall);
                MessageHelper.RunDispatcherLoop();
            }

            Assert.IsNotNull(currentCall);
            Assert.IsTrue(currentCall.Status >= CallStatus.Initiated);
            Assert.AreEqual(Call.CallDirection.Outgoing, currentCall.Direction);
        }


        [TestMethod()]
        public void DialWithNullAddressTest()
        {
            currentCall = null;
            bool result = DialCall("", MediaOption.AudioVideoShare(), ref currentCall);
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void DialWithNullOptionTest()
        {
            currentCall = null;
            bool result = DialCall("123", null, ref currentCall);
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void DialFailWhenNotRegisterPhoneTest()
        {
            DeregisterPhone();

            var callee = fixture.CreatUser();
            Assert.IsNotNull(callee);

            currentCall = null;
            bool result = DialCall(callee.Email, MediaOption.AudioVideoShare(), ref currentCall);
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void DialFailWhenAlreadyHaveCallTest()
        {
            var callee1 = fixture.CreatUser();
            var callee2 = fixture.CreatUser();
            Assert.IsNotNull(callee1);
            Assert.IsNotNull(callee2);

            currentCall = null;
            bool result = DialCall(callee1.Email, MediaOption.AudioVideoShare(), ref currentCall);
            Assert.IsTrue(result);

            result = DialCall(callee2.Email, MediaOption.AudioVideoShare(), ref currentCall);
            Assert.IsFalse(result);

            HangupCall(currentCall);
            MessageHelper.RunDispatcherLoop();
        }

        [TestMethod()]
        public void OutgoingCallStateEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            bool ringSignal = false;
            bool connectSignal = false;
            bool disconnectSignal = false;
            currentCall = null;

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnRinging += (call) =>
                    {
                        Console.WriteLine("CurrentCall_onRinging");
                        ringSignal = true;
                    };

                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("CurrentCall_onConnected");
                        connectSignal = true;
                    };

                    currentCall.OnDisconnected += (releaseReason) =>
                    {
                        Console.WriteLine("CurrentCall_onDisconnected");
                        disconnectSignal = true;
                        callData.ReleaseReason = releaseReason;
                        MessageHelper.BreakLoop();
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();
            Assert.IsTrue(ringSignal);
            Assert.IsTrue(connectSignal);
            Assert.IsTrue(disconnectSignal);
            Assert.IsTrue(callData.ReleaseReason is RemoteLeft);
        }

        [TestMethod()]
        public void OutgoingCallMembershipJoinedEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            CallMembership caller = new CallMembership();
            CallMembership callee = new CallMembership();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnCallMembershipChanged += (callMembershipEvent) =>
                    {
                        Console.WriteLine($"event:{callMembershipEvent.GetType().Name}, callmembership:{callMembershipEvent.CallMembership.Email}");

                        if (callMembershipEvent is CallMembershipJoinedEvent)
                        {
                            if (self.Emails[0] == callMembershipEvent.CallMembership.Email)
                            {
                                caller = callMembershipEvent.CallMembership;
                            }
                            else
                            {
                                callee = callMembershipEvent.CallMembership;
                            }
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(caller.IsInitiator);
            Assert.AreEqual(CallMembership.CallState.Joined, caller.State);
            Assert.AreEqual(self.Id, caller.PersonId);
            Assert.AreEqual(self.Emails[0], caller.Email);

            Assert.IsFalse(callee.IsInitiator);
            Assert.AreEqual(CallMembership.CallState.Joined, callee.State);
            Assert.AreEqual(calleeAddress, callee.Email);
        }

        [TestMethod()]
        public void OutgoingCallMembershipLeftEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            CallMembership caller = new CallMembership();
            CallMembership callee = new CallMembership();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnCallMembershipChanged += (callMembershipEvent) =>
                    {
                        Console.WriteLine($"event:{callMembershipEvent.GetType().Name}, callmembership:{callMembershipEvent.CallMembership.Email}");

                        if (callMembershipEvent is CallMembershipLeftEvent)
                        {
                            if (self.Emails[0] == callMembershipEvent.CallMembership.Email)
                            {
                                caller = callMembershipEvent.CallMembership;
                            }
                            else
                            {
                                callee = callMembershipEvent.CallMembership;
                            }
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(caller.IsInitiator);
            Assert.AreEqual(CallMembership.CallState.Left, caller.State);
            Assert.AreEqual(self.Id, caller.PersonId);
            Assert.AreEqual(self.Emails[0], caller.Email);

            Assert.IsFalse(callee.IsInitiator);
            Assert.AreEqual(CallMembership.CallState.Left, callee.State);
            Assert.AreEqual(calleeAddress, callee.Email);
        }

        [TestMethod()]
        public void OutgoingCallMembershipEventDeclineTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: decline
            MessageHelper.SetTestMode_CalleeAutoDecline(testFixtureApp);

            currentCall = null;
            CallMembership callee = new CallMembership();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (releaseReason) =>
                    {
                        Console.WriteLine("onDisconnected");
                        callData.ReleaseReason = releaseReason;
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnCallMembershipChanged += (callMembershipEvent) =>
                    {
                        Console.WriteLine($"event:{callMembershipEvent.GetType().Name}, callmembership:{callMembershipEvent.CallMembership.Email}");

                        if (callMembershipEvent is CallMembershipDeclinedEvent)
                        {
                            callee = callMembershipEvent.CallMembership;
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsFalse(callee.IsInitiator);
            Assert.AreEqual(calleeAddress, callee.Email);
            Assert.AreEqual(CallMembership.CallState.Declined, callee.State);
            Assert.IsTrue(callData.ReleaseReason is RemoteDecline);
        }

        [TestMethod()]
        public void OutgoingCallMembershipSendingVideoEventMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: mute video
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteVideoAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<CallMembership> callee = new List<CallMembership>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnCallMembershipChanged += (callMembershipEvent) =>
                    {
                        Console.WriteLine($"event:{callMembershipEvent.GetType().Name}, callmembership:{callMembershipEvent.CallMembership.Email}");

                        if (callMembershipEvent is CallMembershipSendingVideoEvent)
                        {
                            callee.Add(callMembershipEvent.CallMembership);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();
            Assert.IsTrue(callee.Count > 0);
            Assert.AreEqual(calleeAddress, callee[0].Email);
            Assert.IsFalse(callee[0].IsSendingVideo);
        }

        [TestMethod()]
        public void OutgoingCallMembershipSendingVideoEventUnmuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: mute video
            //4. callee: unmte video
            //5. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteVideoAndUnMuteVideoAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<CallMembership> callee = new List<CallMembership>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnCallMembershipChanged += (callMembershipEvent) =>
                    {
                        Console.WriteLine($"event:{callMembershipEvent.GetType().Name}, callmembership:{callMembershipEvent.CallMembership.Email}");

                        if (callMembershipEvent is CallMembershipSendingVideoEvent)
                        {
                            callee.Add(callMembershipEvent.CallMembership);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, callee.Count);
            Assert.AreEqual(calleeAddress, callee[0].Email);
            Assert.IsFalse(callee[0].IsSendingVideo);
            Assert.AreEqual(calleeAddress, callee[1].Email);
            Assert.IsTrue(callee[1].IsSendingVideo);
        }

        [TestMethod()]
        public void OutgoingCallMembershipSendingAudioEventMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: mute audio
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteAudioAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<CallMembership> callee = new List<CallMembership>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnCallMembershipChanged += (callMembershipEvent) =>
                    {
                        Console.WriteLine($"event:{callMembershipEvent.GetType().Name}, callmembership:{callMembershipEvent.CallMembership.Email}");

                        if (callMembershipEvent is CallMembershipSendingAudioEvent)
                        {
                            callee.Add(callMembershipEvent.CallMembership);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(callee.Count > 0);
            Assert.AreEqual(calleeAddress, callee[0].Email);
            Assert.IsFalse(callee[0].IsSendingAudio);
        }

        [TestMethod()]
        public void OutgoingCallMembershipSendingAudioEventUnMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: mute audio
            //4. callee: unmute audio
            //5. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteAudioAndUnMuteAudioAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<CallMembership> callee = new List<CallMembership>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnCallMembershipChanged += (callMembershipEvent) =>
                    {
                        Console.WriteLine($"event:{callMembershipEvent.GetType().Name}, callmembership:{callMembershipEvent.CallMembership.Email}");

                        if (callMembershipEvent is CallMembershipSendingAudioEvent)
                        {
                            callee.Add(callMembershipEvent.CallMembership);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, callee.Count);
            Assert.AreEqual(calleeAddress, callee[0].Email);
            Assert.IsFalse(callee[0].IsSendingAudio);
            Assert.AreEqual(calleeAddress, callee[1].Email);
            Assert.IsTrue(callee[1].IsSendingAudio);
        }

        [TestMethod()]
        public void OutgoingCallMembershipSendingShareEventStartTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: start share
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndStartShareAndHangupAfter30s(testFixtureApp);

            currentCall = null;
            List<CallMembership> callee = new List<CallMembership>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnCallMembershipChanged += (callMembershipEvent) =>
                    {
                        Console.WriteLine($"event:{callMembershipEvent.GetType().Name}, callmembership:{callMembershipEvent.CallMembership.Email}");

                        if (callMembershipEvent is CallMembershipSendingShareEvent)
                        {
                            callee.Add(callMembershipEvent.CallMembership);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(callee.Count > 0);
            Assert.AreEqual(calleeAddress, callee[0].Email);
            Assert.IsTrue(callee[0].IsSendingShare);
        }


        [TestMethod()]
        public void OutgoingCallMembershipSendingShareEventStopTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: start share
            //4. callee: stop share
            //5. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndStartShare15sAndHangupAfter30s(testFixtureApp);

            currentCall = null;
            List<CallMembership> callee = new List<CallMembership>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnCallMembershipChanged += (callMembershipEvent) =>
                    {
                        Console.WriteLine($"event:{callMembershipEvent.GetType().Name}, callmembership:{callMembershipEvent.CallMembership.Email}");

                        if (callMembershipEvent is CallMembershipSendingShareEvent)
                        {
                            callee.Add(callMembershipEvent.CallMembership);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, callee.Count);
            Assert.AreEqual(calleeAddress, callee[0].Email);
            Assert.IsTrue(callee[0].IsSendingShare);
            Assert.AreEqual(calleeAddress, callee[1].Email);
            Assert.IsFalse(callee[1].IsSendingShare);
        }


        [TestMethod()]
        public void OutgoingMediaChangedVideoReadyVideoSizeChangedEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        mediaEvents.Add(callMediaChangedEvent);
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count > 0);
            Assert.IsTrue(mediaEvents[0] is LocalVideoViewSizeChangedEvent);
            //Assert.IsTrue(mediaEvents[3] is RemoteVideoViewSizeChangedEvent);
        }


        [TestMethod()]
        public void OutgoingMediaChangedRemoteSendingVideoEventMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: mute video
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteVideoAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute self cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine($"{DateTime.Now} onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"{DateTime.Now} event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteSendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsRemoteSendingVideo.Add(currentCall.IsRemoteSendingVideo);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count > 0);
            var mediaevent = mediaEvents[0] as RemoteSendingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);
            mediaevent = mediaEvents[1] as RemoteSendingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsSending);

            Assert.IsTrue(callData.listIsRemoteSendingVideo.Count > 0);
            Assert.IsTrue(callData.listIsRemoteSendingVideo[0]);
            Assert.IsFalse(callData.listIsRemoteSendingVideo[1]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedRemoteSendingVideoEventUnMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: mute video
            //4. callee: unmute video
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteVideoAndUnMuteVideoAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute self cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine($"{DateTime.Now} onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"{DateTime.Now} event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteSendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsRemoteSendingVideo.Add(currentCall.IsRemoteSendingVideo);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            if (mediaEvents.Count >= 3)
            {
                var mediaevent = mediaEvents[0] as RemoteSendingVideoEvent;
                Assert.IsNotNull(mediaevent);
                Assert.IsTrue(mediaevent.IsSending);
                mediaevent = mediaEvents[1] as RemoteSendingVideoEvent;
                Assert.IsNotNull(mediaevent);
                Assert.IsFalse(mediaevent.IsSending);
                mediaevent = mediaEvents[2] as RemoteSendingVideoEvent;
                Assert.IsNotNull(mediaevent);
                Assert.IsTrue(mediaevent.IsSending);

                Assert.IsTrue(callData.listIsRemoteSendingVideo.Count >= 3);
                Assert.IsTrue(callData.listIsRemoteSendingVideo[0]);
                Assert.IsFalse(callData.listIsRemoteSendingVideo[1]);
                Assert.IsTrue(callData.listIsRemoteSendingVideo[2]);
            }
        }

        [TestMethod()]
        public void OutgoingMediaChangedRemoteSendingAudioEventMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: mute audio
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteAudioAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteSendingAudioEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsRemoteSendingAudio.Add(currentCall.IsRemoteSendingAudio);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count >= 2);
            var mediaevent = mediaEvents[0] as RemoteSendingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);
            mediaevent = mediaEvents[1] as RemoteSendingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsSending);

            Assert.IsTrue(callData.listIsRemoteSendingAudio.Count >= 2);
            Assert.IsTrue(callData.listIsRemoteSendingAudio[0]);
            Assert.IsFalse(callData.listIsRemoteSendingAudio[1]);
        }


        [TestMethod()]
        public void OutgoingMediaChangedRemoteSendingAudioEventUnMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: mute audio
            //4. callee: unmute audio
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteAudioAndUnMuteAudioAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteSendingAudioEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsRemoteSendingAudio.Add(currentCall.IsRemoteSendingAudio);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(3, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as RemoteSendingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);
            mediaevent = mediaEvents[1] as RemoteSendingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsSending);
            mediaevent = mediaEvents[2] as RemoteSendingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);

            Assert.AreEqual(3, callData.listIsRemoteSendingAudio.Count);
            Assert.IsTrue(callData.listIsRemoteSendingAudio[0]);
            Assert.IsFalse(callData.listIsRemoteSendingAudio[1]);
            Assert.IsTrue(callData.listIsRemoteSendingAudio[2]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedSendingVideoEventMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute video
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("local mute video");
                        currentCall.IsSendingVideo = false;
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is SendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsSendingVideo.Add(currentCall.IsSendingVideo);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count >= 2);
            var mediaevent = mediaEvents[0] as SendingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);
            mediaevent = mediaEvents[1] as SendingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsSending);

            Assert.IsTrue(callData.listIsSendingVideo.Count >= 2);
            Assert.IsTrue(callData.listIsSendingVideo[0]);
            Assert.IsFalse(callData.listIsSendingVideo[1]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedSendingVideoEventUnMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute video
            //4. caller: unmute video
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("local mute video");
                        currentCall.IsSendingVideo = false;
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is SendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsSendingVideo.Add(currentCall.IsSendingVideo);

                            if (((SendingVideoEvent)callMediaChangedEvent).IsSending == false)
                            {
                                Console.WriteLine("local unmute video");
                                currentCall.IsSendingVideo = true;
                            }
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(3, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as SendingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);
            mediaevent = mediaEvents[1] as SendingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsSending);
            mediaevent = mediaEvents[2] as SendingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);

            Assert.AreEqual(3, callData.listIsSendingVideo.Count);
            Assert.IsTrue(callData.listIsSendingVideo[0]);
            Assert.IsFalse(callData.listIsSendingVideo[1]);
            Assert.IsTrue(callData.listIsSendingVideo[2]);
        }


        [TestMethod()]
        public void OutgoingMediaChangedSendingAudioEventMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute audio
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("local mute audio");
                        currentCall.IsSendingAudio = false;
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is SendingAudioEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsSendingAudio.Add(currentCall.IsSendingAudio);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count > 0);
            var mediaevent = mediaEvents[0] as SendingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsSending);
            Assert.IsTrue(callData.listIsSendingAudio.Count > 0);
            Assert.IsFalse(callData.listIsSendingAudio[0]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedSendingAudioEventUnMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute audio
            //4. caller: unmute audio
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("local mute audio");
                        currentCall.IsSendingAudio = false;
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is SendingAudioEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsSendingAudio.Add(currentCall.IsSendingAudio);

                            if (((SendingAudioEvent)callMediaChangedEvent).IsSending == false)
                            {
                                Console.WriteLine("local unmute audio");
                                currentCall.IsSendingAudio = true;
                            }
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as SendingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsSending);
            mediaevent = mediaEvents[1] as SendingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);
            Assert.AreEqual(2, callData.listIsSendingAudio.Count);
            Assert.IsFalse(callData.listIsSendingAudio[0]);
            Assert.IsTrue(callData.listIsSendingAudio[1]);
        }
        #region screen share
        //[TestMethod()]
        //public void OutgoingSelectShareSourceDesktopTest()
        //{
        //    //call scene：
        //    //1. caller: callout
        //    //2. callee: answer
        //    //3. caller: select share source
        //    //4. callee: hangup
        //    MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

        //    currentCall = null;
        //    List<ShareSource> shareSources = new List<ShareSource>();

        //    phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
        //    {
        //        if (r.IsSuccess)
        //        {
        //            currentCall = r.Data;
        //            currentCall.OnDisconnected += (call) =>
        //            {
        //                Console.WriteLine("onDisconnected");
        //                MessageHelper.BreakLoop();
        //            };
        //            currentCall.OnConnected += (call) =>
        //            {
        //                Console.WriteLine("onConnected");
        //                Console.WriteLine("select share source");
        //                currentCall.FetchShareSources(ShareSourceType.Desktop, result =>
        //                {
        //                    if (r.IsSuccess)
        //                    {
        //                        shareSources = result.Data;
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($"select share source failed. Error: {result.Error.ErrorCode}: {result.Error.Reason}");
        //                    }
        //                });
        //            };
        //        }
        //        else
        //        {
        //            Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
        //            currentCall = r.Data;
        //            MessageHelper.BreakLoop();
        //        }
        //    });

        //    MessageHelper.RunDispatcherLoop();

        //    Assert.IsTrue(shareSources.Count > 0);
        //    Assert.IsNotNull(shareSources[0].SourceId);
        //    Assert.IsNotNull(shareSources[0].Name);
        //}

        //[TestMethod()]
        //public void OutgoingSelectShareSourceApplicationTest()
        //{
        //    //call scene：
        //    //1. caller: callout
        //    //2. callee: answer
        //    //3. caller: select share source
        //    //4. callee: hangup
        //    MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

        //    currentCall = null;
        //    List<ShareSource> shareSources = new List<ShareSource>();

        //    phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
        //    {
        //        if (r.IsSuccess)
        //        {
        //            currentCall = r.Data;
        //            currentCall.OnDisconnected += (call) =>
        //            {
        //                Console.WriteLine("onDisconnected");
        //                MessageHelper.BreakLoop();
        //            };
        //            currentCall.OnConnected += (call) =>
        //            {
        //                Console.WriteLine("onConnected");
        //                Console.WriteLine("select share source");
        //                currentCall.FetchShareSources(ShareSourceType.Application, result =>
        //                {
        //                    if (r.IsSuccess)
        //                    {
        //                        shareSources = result.Data;
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($"select share source failed. Error: {result.Error.ErrorCode}: {result.Error.Reason}");
        //                    }
        //                });
        //            };
        //        }
        //        else
        //        {
        //            Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
        //            currentCall = r.Data;
        //            MessageHelper.BreakLoop();
        //        }
        //    });

        //    MessageHelper.RunDispatcherLoop();

        //    Assert.IsTrue(shareSources.Count > 0);
        //    Assert.IsNotNull(shareSources[0].SourceId);
        //    Assert.IsNotNull(shareSources[0].Name);
        //}

        //[TestMethod()]
        //public void OutgoingSelectShareSourceDesktopAndApplicationTest()
        //{
        //    //call scene：
        //    //1. caller: callout
        //    //2. callee: answer
        //    //3. caller: select share source
        //    //4. callee: hangup
        //    MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

        //    currentCall = null;
        //    List<ShareSource> shareSources = new List<ShareSource>();

        //    phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
        //    {
        //        if (r.IsSuccess)
        //        {
        //            currentCall = r.Data;
        //            currentCall.OnDisconnected += (call) =>
        //            {
        //                Console.WriteLine("onDisconnected");
        //                MessageHelper.BreakLoop();
        //            };
        //            currentCall.OnConnected += (call) =>
        //            {
        //                Console.WriteLine("onConnected");
        //                Console.WriteLine("select share source");
        //                currentCall.FetchShareSources(ShareSourceType.Desktop, result =>
        //                {
        //                    if (r.IsSuccess)
        //                    {
        //                        shareSources.AddRange(result.Data);
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($"select share source of desktop failed. Error: {result.Error.ErrorCode}: {result.Error.Reason}");
        //                    }
        //                });
        //                currentCall.FetchShareSources(ShareSourceType.Application, result =>
        //                {
        //                    if (r.IsSuccess)
        //                    {
        //                        shareSources.AddRange(result.Data);
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($"select share source of application failed. Error: {result.Error.ErrorCode}: {result.Error.Reason}");
        //                    }
        //                });
        //            };
        //        }
        //        else
        //        {
        //            Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
        //            currentCall = r.Data;
        //            MessageHelper.BreakLoop();
        //        }
        //    });

        //    MessageHelper.RunDispatcherLoop();

        //    Assert.IsTrue(shareSources.Count > 1);
        //    Assert.IsNotNull(shareSources[0].SourceId);
        //    Assert.IsNotNull(shareSources[0].Name);

        //    Assert.IsNotNull(shareSources[1].SourceId);
        //    Assert.IsNotNull(shareSources[1].Name);
        //}

        //[TestMethod()]
        //public void OutgoingShareDesktopTest()
        //{
        //    //call scene：
        //    //1. caller: callout
        //    //2. callee: answer
        //    //3. caller: select share source desktop
        //    //4. caller: start share
        //    //5. callee: hangup
        //    MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

        //    currentCall = null;
        //    List<ShareSource> shareSources = new List<ShareSource>();
        //    List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

        //    phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
        //    {
        //        if (r.IsSuccess)
        //        {
        //            currentCall = r.Data;
        //            currentCall.OnDisconnected += (call) =>
        //            {
        //                Console.WriteLine("onDisconnected");
        //                MessageHelper.BreakLoop();
        //            };
        //            currentCall.OnConnected += (call) =>
        //            {
        //                Console.WriteLine("onConnected");
        //                Console.WriteLine("select share source");
        //                currentCall.FetchShareSources(ShareSourceType.Desktop, result =>
        //                {
        //                    if (result.IsSuccess)
        //                    {
        //                        shareSources = result.Data;
        //                        currentCall.StartShare(shareSources[0].SourceId, startShareResult =>
        //                        {
        //                            Console.WriteLine($"startShare success is {result.IsSuccess}");
        //                        });
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($"select share source failed. Error: {result.Error.ErrorCode}: {result.Error.Reason}");
        //                    }
        //                });
        //            };
        //            currentCall.OnMediaChanged += (callMediaChangedEvent) =>
        //            {
        //                Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
        //                if (callMediaChangedEvent is SendingShareEvent)
        //                {
        //                    mediaEvents.Add(callMediaChangedEvent);
        //                    callData.listIsSendingShare.Add(currentCall.IsSendingShare);
        //                    if (((SendingShareEvent)callMediaChangedEvent).IsSending == true)
        //                    {
        //                        currentCall.StopShare( StopShareResult=>
        //                        {
        //                            Console.WriteLine($"stop share success is {StopShareResult.IsSuccess}");
        //                        });

        //                    }
        //                }
        //            };
        //        }
        //        else
        //        {
        //            Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
        //            currentCall = r.Data;
        //            MessageHelper.BreakLoop();
        //        }
        //    });

        //    MessageHelper.RunDispatcherLoop();

        //    Assert.IsTrue(shareSources.Count > 0);
        //    Assert.IsNotNull(shareSources[0].SourceId);

        //    Assert.AreEqual(2, mediaEvents.Count);
        //    var mediaevent = mediaEvents[0] as SendingShareEvent;
        //    Assert.IsNotNull(mediaevent);
        //    Assert.IsTrue(mediaevent.IsSending);
        //    mediaevent = mediaEvents[1] as SendingShareEvent;
        //    Assert.IsNotNull(mediaevent);
        //    Assert.IsFalse(mediaevent.IsSending);

        //    Assert.AreEqual(2, callData.listIsSendingShare.Count);
        //    Assert.IsTrue(callData.listIsSendingShare[0]);
        //    Assert.IsFalse(callData.listIsSendingShare[1]);
        //}

        //[TestMethod()]
        //public void OutgoingShareApplicationTest()
        //{
        //    //call scene：
        //    //1. caller: callout
        //    //2. callee: answer
        //    //3. caller: select share source desktop
        //    //4. caller: start share
        //    //5. callee: hangup
        //    MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

        //    currentCall = null;
        //    List<ShareSource> shareSources = new List<ShareSource>();
        //    List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

        //    phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
        //    {
        //        if (r.IsSuccess)
        //        {
        //            currentCall = r.Data;
        //            currentCall.OnDisconnected += (call) =>
        //            {
        //                Console.WriteLine("onDisconnected");
        //                MessageHelper.BreakLoop();
        //            };
        //            currentCall.OnConnected += (call) =>
        //            {
        //                Console.WriteLine("onConnected");
        //                Console.WriteLine("select share source");
        //                currentCall.FetchShareSources(ShareSourceType.Application, result =>
        //                {
        //                    if (result.IsSuccess)
        //                    {
        //                        shareSources = result.Data;
        //                        currentCall.StartShare(shareSources[0].SourceId, startShareResult =>
        //                        {
        //                            Console.WriteLine($"startShare success is {startShareResult.IsSuccess}");
        //                        });
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($"select share source failed. Error: {result.Error.ErrorCode}: {result.Error.Reason}");
        //                    }
        //                });
        //            };
        //            currentCall.OnMediaChanged += (callMediaChangedEvent) =>
        //            {
        //                Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
        //                if (callMediaChangedEvent is SendingShareEvent)
        //                {
        //                    mediaEvents.Add(callMediaChangedEvent);
        //                    callData.listIsSendingShare.Add(currentCall.IsSendingShare);
        //                    if (((SendingShareEvent)callMediaChangedEvent).IsSending == true)
        //                    {
        //                        currentCall.StopShare(StopShareResult =>
        //                        {
        //                            Console.WriteLine($"stop share success is {StopShareResult.IsSuccess}");
        //                        });

        //                    }
        //                }
        //            };
        //        }
        //        else
        //        {
        //            Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
        //            currentCall = r.Data;
        //            MessageHelper.BreakLoop();
        //        }
        //    });

        //    MessageHelper.RunDispatcherLoop();

        //    Assert.IsTrue(shareSources.Count > 0);
        //    Assert.IsNotNull(shareSources[0].SourceId);

        //    Assert.AreEqual(2, mediaEvents.Count);
        //    var mediaevent = mediaEvents[0] as SendingShareEvent;
        //    Assert.IsNotNull(mediaevent);
        //    Assert.IsTrue(mediaevent.IsSending);
        //    mediaevent = mediaEvents[1] as SendingShareEvent;
        //    Assert.IsNotNull(mediaevent);
        //    Assert.IsFalse(mediaevent.IsSending);

        //    Assert.AreEqual(2, callData.listIsSendingShare.Count);
        //    Assert.IsTrue(callData.listIsSendingShare[0]);
        //    Assert.IsFalse(callData.listIsSendingShare[1]);
        //}

        //[TestMethod()]
        //public void OutgoingShareApplicationMultiTimesTest()
        //{
        //    //call scene：
        //    //1. caller: callout
        //    //2. callee: answer
        //    //3. caller: select share source desktop
        //    //4. caller: start share desktop
        //    //5. caller: start share another application
        //    //6. callee: hangup
        //    MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

        //    currentCall = null;
        //    List<ShareSource> shareSources = new List<ShareSource>();
        //    List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

        //    phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
        //    {
        //        if (r.IsSuccess)
        //        {
        //            currentCall = r.Data;
        //            currentCall.OnDisconnected += (call) =>
        //            {
        //                Console.WriteLine("onDisconnected");
        //                MessageHelper.BreakLoop();
        //            };
        //            currentCall.OnConnected += (call) =>
        //            {
        //                Console.WriteLine("onConnected");
        //                Console.WriteLine("select share source");
        //                currentCall.FetchShareSources(ShareSourceType.Application, result =>
        //                {
        //                    if (result.IsSuccess)
        //                    {
        //                        shareSources = result.Data;
        //                        currentCall.StartShare(shareSources[0].SourceId, startShareResult =>
        //                        {
        //                            Console.WriteLine($"startShare success is {result.IsSuccess}");
        //                        });
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($"select share source failed. Error: {result.Error.ErrorCode}: {result.Error.Reason}");
        //                    }
        //                });
        //            };
        //            currentCall.OnMediaChanged += (callMediaChangedEvent) =>
        //            {
        //                Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
        //                if (callMediaChangedEvent is SendingShareEvent)
        //                {
        //                    mediaEvents.Add(callMediaChangedEvent);
        //                    callData.listIsSendingShare.Add(currentCall.IsSendingShare);
        //                    if (shareSources.Count >1 && ((SendingShareEvent)callMediaChangedEvent).IsSending == true)
        //                    {
        //                        currentCall.StartShare(shareSources[1].SourceId, startShareResult =>
        //                        {
        //                            Console.WriteLine($"startShare success is {startShareResult.IsSuccess}");
        //                        });

        //                    }
        //                }
        //            };
        //        }
        //        else
        //        {
        //            Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
        //            currentCall = r.Data;
        //            MessageHelper.BreakLoop();
        //        }
        //    });

        //    MessageHelper.RunDispatcherLoop();

        //    Assert.IsTrue(shareSources.Count > 0);
        //    Assert.IsNotNull(shareSources[0].SourceId);
        //    if (shareSources.Count > 1)
        //    {
        //        Assert.IsNotNull(shareSources[1].SourceId);
        //        Assert.AreEqual(2, mediaEvents.Count);
        //        var mediaevent1 = mediaEvents[1] as SendingShareEvent;
        //        Assert.IsNotNull(mediaevent1);
        //        Assert.IsTrue(mediaevent1.IsSending);

        //        Assert.AreEqual(2, callData.listIsSendingShare.Count);
        //        Assert.IsTrue(callData.listIsSendingShare[1]);
        //    }

        //    var mediaevent = mediaEvents[0] as SendingShareEvent;
        //    Assert.IsNotNull(mediaevent);
        //    Assert.IsTrue(mediaevent.IsSending);        
        //    Assert.IsTrue(callData.listIsSendingShare[0]);       
        //}
        #endregion

        [TestMethod()]
        public void OutgoingMediaChangedReceivingVideoEventMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute remote video
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("local mute remote video");
                        currentCall.IsReceivingVideo = false;
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is ReceivingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsReceivingVideo.Add(currentCall.IsReceivingVideo);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(1, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as ReceivingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsReceiving);
            Assert.IsTrue(callData.listIsReceivingVideo.Count > 0);
            Assert.IsFalse(callData.listIsReceivingVideo[0]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedReceivingVideoEventUnMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute remote video
            //4. caller: unmute remote video
            //5. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("local mute remote video");
                        currentCall.IsReceivingVideo = false;
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is ReceivingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsReceivingVideo.Add(currentCall.IsReceivingVideo);

                            if (((ReceivingVideoEvent)callMediaChangedEvent).IsReceiving == false)
                            {
                                Console.WriteLine("local unmute remote video");
                                currentCall.IsReceivingVideo = true;
                            }
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as ReceivingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsReceiving);
            mediaevent = mediaEvents[1] as ReceivingVideoEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsReceiving);
            Assert.AreEqual(2, callData.listIsReceivingVideo.Count);
            Assert.IsFalse(callData.listIsReceivingVideo[0]);
            Assert.IsTrue(callData.listIsReceivingVideo[1]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedReceivingAudioEventMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute remote audio
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("local mute remote audio");
                        currentCall.IsReceivingAudio = false;
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is ReceivingAudioEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsReceivingAudio.Add(currentCall.IsReceivingAudio);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(1, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as ReceivingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsReceiving);
            Assert.IsTrue(callData.listIsReceivingAudio.Count > 0);
            Assert.IsFalse(callData.listIsReceivingAudio[0]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedReceivingAudioEventUnMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute remote audio
            //4. caller: unmute remote audio
            //5. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("local mute remote audio");
                        currentCall.IsReceivingAudio = false;
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is ReceivingAudioEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsReceivingAudio.Add(currentCall.IsReceivingAudio);

                            if (((ReceivingAudioEvent)callMediaChangedEvent).IsReceiving == false)
                            {
                                Console.WriteLine("local unmute remote audio");
                                currentCall.IsReceivingAudio = true;
                            }
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as ReceivingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsReceiving);
            mediaevent = mediaEvents[1] as ReceivingAudioEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsReceiving);
            Assert.AreEqual(2, callData.listIsReceivingAudio.Count);
            Assert.IsFalse(callData.listIsReceivingAudio[0]);
            Assert.IsTrue(callData.listIsReceivingAudio[1]);
        }


        [TestMethod()]
        public void OutgoingMediaChangedCameraSwitchedEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: switch camera
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            var cameras = phone.GetAVIODevices(AVIODeviceType.Camera);
            if (cameras.Count < 3)
            {
                //there is a default camera item in list, so default the list count is 2.
                Console.WriteLine("need at least two cameras.");
                //Assert.Fail();
                return;
            }
            phone.SelectAVIODevice(cameras[1]);
            var switchtoCamera = cameras[2];

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("switch camera");
                        phone.SelectAVIODevice(switchtoCamera);

                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is CameraSwitchedEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();
            phone.SelectAVIODevice(cameras[0]);

            Assert.AreEqual(1, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as CameraSwitchedEvent;
            Assert.IsNotNull(mediaevent);
            Assert.AreEqual(switchtoCamera.Id, mediaevent.Camera.Id);
            Assert.AreEqual(switchtoCamera.Type, mediaevent.Camera.Type);
            Assert.AreEqual(switchtoCamera.Name, mediaevent.Camera.Name);
        }

        [TestMethod()]
        public void OutgoingMediaChangedSpearkerSwitchedEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: switch speaker
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            var speakers = phone.GetAVIODevices(AVIODeviceType.Speaker);
            if (speakers.Count < 3)
            {
                //there is a default speaker item in list, so default the list count is 2.
                Console.WriteLine("need at least two speakers.");
                //Assert.Fail();
                return;
            }
            phone.SelectAVIODevice(speakers[1]);
            var switchtoSpeaker = speakers[2];

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        Console.WriteLine("switch speaker");
                        phone.SelectAVIODevice(switchtoSpeaker);

                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is SpeakerSwitchedEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });

            MessageHelper.RunDispatcherLoop();

            phone.SelectAVIODevice(speakers[0]);

            Assert.AreEqual(1, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as SpeakerSwitchedEvent;
            Assert.IsNotNull(mediaevent);
            Assert.AreEqual(switchtoSpeaker.Id, mediaevent.Speaker.Id);
            Assert.AreEqual(switchtoSpeaker.Type, mediaevent.Speaker.Type);
            Assert.AreEqual(switchtoSpeaker.Name, mediaevent.Speaker.Name);
        }



        [TestMethod()]
        public void OutgoingMediaChangedRemoteSendingShareEventStartTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: start share
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndStartShareAndHangupAfter30s(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteSendingShareEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsRemoteSendingShare.Add(currentCall.IsRemoteSendingShare);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count > 0);
            var mediaevent = mediaEvents[0] as RemoteSendingShareEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);
            Assert.IsTrue(callData.listIsRemoteSendingShare.Count > 0);
            Assert.IsTrue(callData.listIsRemoteSendingShare[0]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedRemoteSendingShareEventStopTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: start share
            //4. callee: stop share
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndStartShare15sAndHangupAfter30s(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteSendingShareEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsRemoteSendingShare.Add(currentCall.IsRemoteSendingShare);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as RemoteSendingShareEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsSending);
            mediaevent = mediaEvents[1] as RemoteSendingShareEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsSending);
            Assert.AreEqual(2, callData.listIsRemoteSendingShare.Count);
            Assert.IsTrue(callData.listIsRemoteSendingShare[0]);
            Assert.IsFalse(callData.listIsRemoteSendingShare[1]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedReceivingShareEventMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: start share
            //4. caller: mute remote share
            //5. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndStartShareAndHangupAfter30s(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is ReceivingShareEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsReceivingShare.Add(currentCall.IsReceivingShare);
                        }
                        if (callMediaChangedEvent is RemoteSendingShareEvent
                            && ((RemoteSendingShareEvent)callMediaChangedEvent).IsSending == true)
                        {
                            Console.WriteLine("mute remote share");
                            currentCall.IsReceivingShare = false;
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(1, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as ReceivingShareEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsReceiving);
            Assert.IsTrue(callData.listIsReceivingShare.Count > 0);
            Assert.IsFalse(callData.listIsReceivingShare[0]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedReceivingShareEventUnMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: start share
            //4. caller: mute remote share
            //5. caller: unmute remote share
            //6. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndStartShareAndHangupAfter30s(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;
                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is ReceivingShareEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            callData.listIsReceivingShare.Add(currentCall.IsReceivingShare);

                            if (((ReceivingShareEvent)callMediaChangedEvent).IsReceiving == false)
                            {
                                Console.WriteLine("unmute remote share");
                                currentCall.IsReceivingShare = true;
                            }

                        }
                        if (callMediaChangedEvent is RemoteSendingShareEvent
                            && ((RemoteSendingShareEvent)callMediaChangedEvent).IsSending == true)
                        {
                            Console.WriteLine("mute remote share");
                            currentCall.IsReceivingShare = false;
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, mediaEvents.Count);
            var mediaevent = mediaEvents[0] as ReceivingShareEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsFalse(mediaevent.IsReceiving);
            mediaevent = mediaEvents[1] as ReceivingShareEvent;
            Assert.IsNotNull(mediaevent);
            Assert.IsTrue(mediaevent.IsReceiving);
            Assert.AreEqual(2, callData.listIsReceivingShare.Count);
            Assert.IsFalse(callData.listIsReceivingShare[0]);
            Assert.IsTrue(callData.listIsReceivingShare[1]);
        }

        [TestMethod()]
        public void OutgoingSubscribeRemoteAuxVideoWhenCallConnectedTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<Call.RemoteAuxVideo> remoteAuxVideos = new List<Call.RemoteAuxVideo>();
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();
            List<bool> remoteAuxSendingVideos = new List<bool>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        var remoteAuxVideo = currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        while (remoteAuxVideo != null)
                        {
                            remoteAuxVideo = currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                        remoteAuxVideos = currentCall.RemoteAuxVideos;
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxSendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            var remoteAuxSendingVideoEvent = callMediaChangedEvent as RemoteAuxSendingVideoEvent;
                            remoteAuxSendingVideos.Add(remoteAuxSendingVideoEvent.RemoteAuxVideo.IsSendingVideo);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(4, remoteAuxVideos.Count);
            //Assert.IsTrue(mediaEvents.Count > 0);//unstable
            //Assert.IsTrue(remoteAuxSendingVideos[0]);
        }

        [TestMethod()]
        public void OutgoingSubscribeRemoteAuxVideoWhenCallStartTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            int remoteAuxVideoCount = 0;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    var remoteAuxVideo = currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                    while (remoteAuxVideo != null)
                    {
                        remoteAuxVideoCount++;
                        remoteAuxVideo = currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                    }

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };

                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxSendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            // subscribing remote auxiliary video should be invoked when RemoteAuxVideosCountChangedEvent.
            Assert.AreEqual(0, remoteAuxVideoCount);
            Assert.AreEqual(0, mediaEvents.Count);
        }

        [TestMethod()]
        public void OutgoingUnSubscribeRemoteAuxVideoTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<Call.RemoteAuxVideo> remoteAuxVideos = new List<Call.RemoteAuxVideo>();
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnConnected += (call) =>
                    {
                        Console.WriteLine("onConnected");
                        var remoteAuxVideo = currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        while (remoteAuxVideo != null)
                        {
                            remoteAuxVideo = currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                        remoteAuxVideos = new List<Call.RemoteAuxVideo>(currentCall.RemoteAuxVideos);
                        foreach (var item in remoteAuxVideos)
                        {
                            currentCall.UnsubscribeRemoteAuxVideo(item);
                        }
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxSendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(0, currentCall.RemoteAuxVideos.Count);
            Assert.AreEqual(0, mediaEvents.Count);
        }

        [TestMethod()]
        public void OutgoingMediaChangedRemoteAuxVideosCountChangedEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxVideosCountChangedEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, mediaEvents.Count);
            var remoteAuxVideosCountChangedEvent = mediaEvents[0] as RemoteAuxVideosCountChangedEvent;
            Assert.IsNotNull(remoteAuxVideosCountChangedEvent);
            Assert.IsTrue(remoteAuxVideosCountChangedEvent.Count == 1);

            remoteAuxVideosCountChangedEvent = mediaEvents[1] as RemoteAuxVideosCountChangedEvent;
            Assert.IsNotNull(remoteAuxVideosCountChangedEvent);
            Assert.IsTrue(remoteAuxVideosCountChangedEvent.Count == 0);
        }

        [TestMethod()]
        public void OutgoingMediaChangedRemoteAuxVideoPersonChangedEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();
            List<CallMembership> perosons = new List<CallMembership>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxVideosCountChangedEvent)
                        {
                            currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                        if (callMediaChangedEvent is RemoteAuxVideoPersonChangedEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            perosons.Add(((RemoteAuxVideoPersonChangedEvent)callMediaChangedEvent).RemoteAuxVideo.Person);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.AreEqual(2, mediaEvents.Count);

            var remoteAuxVideoPersonChangedEvent = mediaEvents[0] as RemoteAuxVideoPersonChangedEvent;
            Assert.IsNotNull(perosons[0]);
            Assert.IsNotNull(perosons[0].Email);
            remoteAuxVideoPersonChangedEvent = mediaEvents[1] as RemoteAuxVideoPersonChangedEvent;
            Assert.IsNull(perosons[1]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedRemoteAuxVideoSizeChangedEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxVideosCountChangedEvent)
                        {
                            currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                        if (callMediaChangedEvent is RemoteAuxVideoSizeChangedEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count > 0);

            var remoteAuxVideoSizeChangedEvent = mediaEvents[0] as RemoteAuxVideoSizeChangedEvent;
            Assert.IsNotNull(remoteAuxVideoSizeChangedEvent);
            Assert.IsNotNull(remoteAuxVideoSizeChangedEvent.RemoteAuxVideo.RemoteAuxVideoSize);
            Assert.IsNotNull(remoteAuxVideoSizeChangedEvent.RemoteAuxVideo.RemoteAuxVideoSize.Width);
            Assert.IsNotNull(remoteAuxVideoSizeChangedEvent.RemoteAuxVideo.RemoteAuxVideoSize.Height);
        }

        [TestMethod()]
        public void OutgoingMediaChangedRemoteAuxSendingVideoEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();
            List<bool> remoteAuxSendingVideos = new List<bool>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxVideosCountChangedEvent)
                        {
                            currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                        if (callMediaChangedEvent is RemoteAuxSendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            var remoteAuxSendingVideoEvent = callMediaChangedEvent as RemoteAuxSendingVideoEvent;
                            remoteAuxSendingVideos.Add(remoteAuxSendingVideoEvent.RemoteAuxVideo.IsSendingVideo);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count > 0);
            Assert.IsTrue(remoteAuxSendingVideos.Count > 0);
            Assert.IsTrue(remoteAuxSendingVideos[0]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedRemoteAuxSendingVideoEventByRemoteMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //2. callee: mute
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteVideoAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();
            List<bool> remoteAuxSendingVideos = new List<bool>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine($"{DateTime.Now} onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"{DateTime.Now} event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxVideosCountChangedEvent)
                        {
                            currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                        if (callMediaChangedEvent is RemoteAuxSendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            var remoteAuxSendingVideoEvent = callMediaChangedEvent as RemoteAuxSendingVideoEvent;
                            remoteAuxSendingVideos.Add(remoteAuxSendingVideoEvent.RemoteAuxVideo.IsSendingVideo);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            //comment for not stable
            //Assert.AreEqual(2, mediaEvents.Count);
            //Assert.AreEqual(2, remoteAuxSendingVideos.Count);
            //Assert.IsTrue(remoteAuxSendingVideos[0]);
            //Assert.IsFalse(remoteAuxSendingVideos[1]);
        }
        [TestMethod()]
        public void OutgoingMediaChangedRemoteAuxSendingVideoEventByRemoteUnMuteTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //2. callee: mute
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndMuteVideoAndUnMuteVideoAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();
            List<bool> remoteAuxSendingVideos = new List<bool>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine($"{DateTime.Now} onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"{DateTime.Now} event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxVideosCountChangedEvent)
                        {
                            currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                        if (callMediaChangedEvent is RemoteAuxSendingVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            var remoteAuxSendingVideoEvent = callMediaChangedEvent as RemoteAuxSendingVideoEvent;
                            remoteAuxSendingVideos.Add(remoteAuxSendingVideoEvent.RemoteAuxVideo.IsSendingVideo);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            //comment for not stable
            //Assert.IsTrue(mediaEvents.Count >= 3);
            //Assert.IsTrue(remoteAuxSendingVideos.Count >= 3);
            //Assert.IsTrue(remoteAuxSendingVideos[0]);
            //Assert.IsFalse(remoteAuxSendingVideos[1]);
            //Assert.IsTrue(remoteAuxSendingVideos[2]);
        }

        [TestMethod()]
        public void OutgoingMediaChangedReceivingAuxVideoEventMuteRemoteAuxTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute remote aux1
            //4. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();
            List<bool> receivingAuxVideos = new List<bool>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxVideosCountChangedEvent)
                        {
                            currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                        if (callMediaChangedEvent is RemoteAuxSendingVideoEvent)
                        {
                            var remoteAuxSendingVideoEvent = callMediaChangedEvent as RemoteAuxSendingVideoEvent;
                            if (remoteAuxSendingVideoEvent.RemoteAuxVideo.IsSendingVideo)
                            {
                                remoteAuxSendingVideoEvent.RemoteAuxVideo.IsReceivingVideo = false;
                            }
                        }
                        if (callMediaChangedEvent is ReceivingAuxVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            var receivingAuxVideoEvent = callMediaChangedEvent as ReceivingAuxVideoEvent;
                            receivingAuxVideos.Add(receivingAuxVideoEvent.RemoteAuxVideo.IsReceivingVideo);
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count > 0);
            Assert.AreEqual(1, receivingAuxVideos.Count);
            Assert.IsFalse(receivingAuxVideos[0]);
        }
        [TestMethod()]
        public void OutgoingMediaChangedReceivingAuxVideoEventUnMuteRemoteAuxTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. caller: mute remote aux1
            //4. caller: unmute remote aux1
            //5. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();
            List<bool> receivingAuxVideos = new List<bool>();

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");
                        if (callMediaChangedEvent is RemoteAuxVideosCountChangedEvent)
                        {
                            currentCall.SubscribeRemoteAuxVideo(IntPtr.Zero);
                        }
                        if (callMediaChangedEvent is RemoteAuxSendingVideoEvent)
                        {
                            var remoteAuxSendingVideoEvent = callMediaChangedEvent as RemoteAuxSendingVideoEvent;
                            if (remoteAuxSendingVideoEvent.RemoteAuxVideo.IsSendingVideo)
                            {
                                remoteAuxSendingVideoEvent.RemoteAuxVideo.IsReceivingVideo = false;
                            }
                        }
                        if (callMediaChangedEvent is ReceivingAuxVideoEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            var receivingAuxVideoEvent = callMediaChangedEvent as ReceivingAuxVideoEvent;
                            receivingAuxVideos.Add(receivingAuxVideoEvent.RemoteAuxVideo.IsReceivingVideo);
                            if (receivingAuxVideoEvent.RemoteAuxVideo.IsReceivingVideo == false)
                            {
                                receivingAuxVideoEvent.RemoteAuxVideo.IsReceivingVideo = true;
                            }
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count > 0);
            Assert.AreEqual(2, receivingAuxVideos.Count);
            Assert.IsFalse(receivingAuxVideos[0]);
            Assert.IsTrue(receivingAuxVideos[1]);
        }


        [TestMethod()]
        public void OutgoingMediaChangedActiveSpeakerChangedEventTest()
        {
            //call scene：
            //1. caller: callout
            //2. callee: answer
            //3. callee: hangup
            MessageHelper.SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(testFixtureApp);

            currentCall = null;
            List<MediaChangedEvent> mediaEvents = new List<MediaChangedEvent>();
            bool isActiveSpeaker = false;

            phone.Dial(calleeAddress, MediaOption.AudioVideoShare(), r =>
            {
                if (r.IsSuccess)
                {
                    currentCall = r.Data;

                    //mute local video, cause there is only one camera which should be used by callee part.
                    currentCall.IsSendingVideo = false;

                    currentCall.OnDisconnected += (call) =>
                    {
                        Console.WriteLine("onDisconnected");
                        MessageHelper.BreakLoop();
                    };
                    currentCall.OnMediaChanged += (callMediaChangedEvent) =>
                    {
                        Console.WriteLine($"event:{callMediaChangedEvent.GetType().Name}");

                        if (callMediaChangedEvent is ActiveSpeakerChangedEvent)
                        {
                            mediaEvents.Add(callMediaChangedEvent);
                            isActiveSpeaker = currentCall.To.IsActiveSpeaker;
                        }
                    };
                }
                else
                {
                    Console.WriteLine($"dial fail: {r.Error.ErrorCode}:{r.Error.Reason}");
                    currentCall = r.Data;
                    MessageHelper.BreakLoop();
                }
            });


            MessageHelper.RunDispatcherLoop();

            Assert.IsTrue(mediaEvents.Count > 0);

            var activeSpeakerChangedEvent = mediaEvents[0] as ActiveSpeakerChangedEvent;
            Assert.IsNotNull(activeSpeakerChangedEvent);
            Assert.IsNotNull(activeSpeakerChangedEvent.ActiveSpeaker);
            Assert.IsTrue(isActiveSpeaker);
        }

        [TestMethod()]
        public void IncomingCallStateEventTest()
        {
            phone.OnIncoming += Phone_onIncomingCallStateEventTest;

            //call scene：
            //1. remote: callout
            //2. local: answer
            //3. local: hangup
            MessageHelper.SetTestMode_RemoteDialout(testFixtureApp, self.Id);
            MessageHelper.RunDispatcherLoop();

            phone.OnIncoming -= Phone_onIncomingCallStateEventTest;
            Assert.IsTrue(callData.connectSignal);
            Assert.IsTrue(callData.disconnectSignal);
            Assert.IsTrue(callData.ReleaseReason is LocalLeft);
        }

        private void Phone_onIncomingCallStateEventTest(Call obj)
        {
            currentCall = obj;
            currentCall.Acknowledge(r =>
            { });

            currentCall.OnConnected += (call) =>
            {
                Console.WriteLine("CurrentCall_onConnected");
                callData.connectSignal = true;
                HangupCall(currentCall);
            };

            currentCall.OnDisconnected += (releaseReason) =>
            {
                Console.WriteLine("CurrentCall_onDisconnected");
                callData.disconnectSignal = true;
                callData.ReleaseReason = releaseReason;
                MessageHelper.BreakLoop();
            };

            currentCall.Answer(MediaOption.AudioVideoShare(), r =>
            {
                if (!r.IsSuccess)
                {
                    Console.WriteLine($"Phone_onIncomingCallStateEventTest: answer fail. {r.Error?.ErrorCode} {r.Error?.Reason}");
                }
            });
        }

        [TestMethod()]
        public void IncomingCallRejectTest()
        {
            phone.OnIncoming += Phone_onIncomingCallRejectTest;

            //call scene：
            //1. remote: callout
            //2. local: reject
            MessageHelper.SetTestMode_RemoteDialout(testFixtureApp, self.Id);
            MessageHelper.RunDispatcherLoop();

            phone.OnIncoming -= Phone_onIncomingCallRejectTest;
            Assert.IsNotNull(currentCall);
            Assert.IsTrue(callData.ReleaseReason is LocalDecline);
        }

        private void Phone_onIncomingCallRejectTest(Call obj)
        {
            currentCall = obj;
            currentCall.OnDisconnected += (r) =>
            {
                Console.WriteLine("CurrentCall_onDisconnected");
                callData.ReleaseReason = r;
                MessageHelper.BreakLoop();
            };
            TimerHelper.StartTimer(1000, (o, e) =>
            {
                currentCall.Reject(r =>
                {
                    if (!r.IsSuccess)
                    {
                        Console.WriteLine($"Phone_onIncomingCallRejectTest: reject fail. {r.Error?.ErrorCode} {r.Error?.Reason}");
                    }
                });
            });

        }

        [TestMethod()]
        public void DialWithHydraSpaceIdAndHangUpTest()
        {
            currentCall = null;
            bool result = DialCall(mySpace.Id, MediaOption.AudioVideoShare(), ref currentCall);
            Assert.IsTrue(result);
            if (currentCall != null)
            {
                HangupCall(currentCall);
                MessageHelper.RunDispatcherLoop();
            }

            Assert.IsNotNull(currentCall);
            Assert.IsTrue(currentCall.Status >= CallStatus.Initiated);
            Assert.AreEqual(Call.CallDirection.Outgoing, currentCall.Direction);
        }

        [TestMethod()]
        public void RequestVideoCodecActivationTest()
        {
            var completion = new ManualResetEvent(false);
            phone.ActivateVideoCodecLicense(false);
            phone.DisableVideoCodecActivation(false);

            phone.OnRequestVideoCodecActivation += () =>
            {
                Assert.IsNotNull(phone.VideoCodecLicense);
                Assert.AreEqual("http://www.openh264.org/BINARY_LICENSE.txt", phone.VideoCodecLicenseURL);
                phone.ActivateVideoCodecLicense(true);
                phone.ActivateVideoCodecLicense(false);
                completion.Set();
            };

            phone.RequestVideoCodecActivation();

            if (false == completion.WaitOne(30000))
            {
                phone.DisableVideoCodecActivation(true);
                Assert.Fail();
            }
            phone.DisableVideoCodecActivation(true);
        }

        [TestMethod()]
        public void StartPreviewTest()
        {
            phone.StartPreview(IntPtr.Zero);
        }

        [TestMethod()]
        public void StopPreviewTest()
        {
            phone.StopPreview(IntPtr.Zero);
        }

        [TestMethod()]
        public void GetCamerasTest()
        {
            var devices = phone.GetAVIODevices(AVIODeviceType.Camera);
            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count > 0);
            Assert.AreEqual(AVIODeviceType.Camera, devices[0].Type);
            Assert.IsNotNull(devices[0].Id);
            Assert.IsNotNull(devices[0].Name);
        }

        [TestMethod()]
        public void GetSpeakersTest()
        {
            var devices = phone.GetAVIODevices(AVIODeviceType.Speaker);
            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count > 0);
            Assert.AreEqual(AVIODeviceType.Speaker, devices[0].Type);
            Assert.IsNotNull(devices[0].Id);
            Assert.IsNotNull(devices[0].Name);
        }

        [TestMethod()]
        public void GetMicrophonesTest()
        {
            var devices = phone.GetAVIODevices(AVIODeviceType.Microphone);
            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count > 0);
            Assert.AreEqual(AVIODeviceType.Microphone, devices[0].Type);
            Assert.IsNotNull(devices[0].Id);
            Assert.IsNotNull(devices[0].Name);
        }

        [TestMethod()]
        public void GetRingersTest()
        {
            var devices = phone.GetAVIODevices(AVIODeviceType.Ringer);
            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count > 0);
            Assert.AreEqual(AVIODeviceType.Ringer, devices[0].Type);
            Assert.IsNotNull(devices[0].Id);
            Assert.IsNotNull(devices[0].Name);
        }

        [TestMethod()]
        public void SelectCameraTest()
        {
            var devices = phone.GetAVIODevices(AVIODeviceType.Camera);
            Assert.IsNotNull(devices);
            if (devices.Count == 0)
            {
                Console.WriteLine("no camera");
                return;
            }
            Assert.IsTrue(devices.Count > 0);
            Assert.AreEqual(AVIODeviceType.Camera, devices[0].Type);
            Assert.IsNotNull(devices[0].Id);
            Assert.IsNotNull(devices[0].Name);

            Assert.IsTrue(phone.SelectAVIODevice(devices[0]));
        }

        [TestMethod()]
        public void SelectSpeakerTest()
        {
            var devices = phone.GetAVIODevices(AVIODeviceType.Speaker);
            Assert.IsNotNull(devices);
            if (devices.Count == 0)
            {
                Console.WriteLine("no speaker");
                return;
            }
            Assert.IsTrue(devices.Count > 0);
            Assert.AreEqual(AVIODeviceType.Speaker, devices[0].Type);
            Assert.IsNotNull(devices[0].Id);
            Assert.IsNotNull(devices[0].Name);

            Assert.IsTrue(phone.SelectAVIODevice(devices[0]));
        }

        private static bool RegisterPhone()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();

            webex.Phone.Register(rsp =>
            {
                response = rsp;
                completion.Set();
            });
            if (false == completion.WaitOne(120000))
            {
                Console.WriteLine("registerPhone out of time.");
                return false;
            }

            if (response.IsSuccess == true)
            {
                Console.WriteLine("registerPhone success.");
                return true;
            }

            Console.WriteLine($"registerPhone fail for {response.Error.ErrorCode.ToString()}");
            return false;
        }

        private bool DeregisterPhone()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();

            webex.Phone.Deregister(rsp =>
            {
                response = rsp;
                completion.Set();
            });
            if (false == completion.WaitOne(30000))
            {
                Console.WriteLine("deregister timeout");
                return false;
            }

            if (response.IsSuccess == true)
            {
                return true;
            }

            return false;
        }

        private bool DialCall(string address, MediaOption mediaOption, ref Call call)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Call>();

            phone.Dial(address, mediaOption, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (false == completion.WaitOne(30000))
            {
                Console.WriteLine("dialCall out of time");
                return false;
            }

            if (response.IsSuccess)
            {
                call = response.Data;
                call.OnDisconnected += (r) =>
                {
                    Console.WriteLine("onDisconnected");
                    callData.ReleaseReason = r;
                    MessageHelper.BreakLoop();
                };
                return true;
            }
            else
            {
                Console.WriteLine($"dial fail: {response.Error.ErrorCode}:{response.Error.Reason}");
                call = response.Data;
                return false;
            }
        }

        private void HangupCall(Call call, int after = 5000)
        {
            if (call != null)
            {
                TimerHelper.StartTimer(after, (o, e) =>
                {
                    call?.Hangup(completedHandler =>
                    { });
                });
            }
        }

        private static Space CreateSpace(string title)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Space>();
            webex.Spaces.Create(title, null, rsp =>
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

        private static bool DeleteSpace(string spaceId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
            webex.Spaces.Delete(spaceId, rsp =>
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

        private Membership CreateMembership(string spaceId, string email, string personId, bool isModerator)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Membership>();
            if (email != null)
            {
                webex.Memberships.CreateByPersonEmail(spaceId, email, isModerator, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }
            else
            {
                webex.Memberships.CreateByPersonId(spaceId, personId, isModerator, rsp =>
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

        private List<Membership> ListMembership(string spaceId, int? max = null)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<List<Membership>>();
            webex.Memberships.List(spaceId, max, rsp =>
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

    public class CallData
    {
        public bool ringSignal;
        public bool connectSignal;
        public bool disconnectSignal;

        public List<bool> listIsRemoteSendingVideo;
        public List<bool> listIsRemoteSendingAudio;
        public List<bool> listIsRemoteSendingShare;
        public List<bool> listIsSendingVideo;
        public List<bool> listIsSendingAudio;
        public List<bool> listIsSendingShare;
        public List<bool> listIsReceivingVideo;
        public List<bool> listIsReceivingAudio;
        public List<bool> listIsReceivingShare;
        public List<bool> listIsSendingDTMFEnabled;

        public CallStatus Status { get; set; }
        public Call.CallDirection Direction { get; set; }


        public CallDisconnectedEvent ReleaseReason { get; set; }

        public List<CallMembershipChangedEvent> listCallMembershipChangedEvent;
        public List<MediaChangedEvent> listMediaChangedEvent;

        public CallData()
        {
            listCallMembershipChangedEvent = new List<CallMembershipChangedEvent>();
            listMediaChangedEvent = new List<MediaChangedEvent>();
            listIsRemoteSendingVideo = new List<bool>();
            listIsRemoteSendingAudio = new List<bool>();
            listIsRemoteSendingShare = new List<bool>();
            listIsSendingVideo = new List<bool>();
            listIsSendingAudio = new List<bool>();
            listIsSendingShare = new List<bool>();
            listIsReceivingVideo = new List<bool>();
            listIsReceivingAudio = new List<bool>();
            listIsReceivingShare = new List<bool>();
            listIsSendingDTMFEnabled = new List<bool>();
        }
    }


}