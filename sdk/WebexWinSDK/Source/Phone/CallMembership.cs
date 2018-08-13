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

namespace WebexSDK
{
    /// <summary>
    /// A data type represents a relationship between *Call* and *Person* at Cisco Webex cloud.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public class CallMembership
    {

        /// <summary>
        /// The enumeration of the status of the person in the membership.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public enum CallState
        {
            /// <summary>
            /// The person status is unknown.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            Unknown,
            /// <summary>
            /// The person is idle w/o any call.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            Idle,
            /// <summary>
            /// The person has been notified about the call.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            Notified,
            /// <summary>
            /// The person has joined the call.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            Joined,
            /// <summary>
            /// The person has left the call.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            Left,
            /// <summary>
            /// The person has declined the call.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            Declined,
        }

        /// <summary>
        /// Gets a value indicating whether the person is initiator.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this person is initiator; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsInitiator { get; set; }


        /// <summary>
        /// Gets the preson identifier.
        /// </summary>
        /// <value>
        /// The preson identifier.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public string PersonId { get; set; }


        /// <summary>
        /// Gets the status of the person in this call
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public CallState State { get; set; }


        /// <summary>
        /// Gets the email address of the person in this CallMembership.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public string Email { get; set; }


        /// <summary>
        /// Gets the SIP address of the person in this CallMembership.
        /// </summary>
        /// <value>
        /// The SIP address.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public string SipUrl { get; set; }


        /// <summary>
        /// Gets the phone number of the person in this CallMembership.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// True if the CallMembership is sending video. Otherwise, false.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsSendingVideo { get; set; }


        /// <summary>
        /// True if the CallMembership is sending audio. Otherwise, false.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsSendingAudio { get; set; }

        /// <summary>
        /// True if the CallMembership is sending share. Otherwise, false.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsSendingShare { get; set; }

        /// <summary>
        /// True if CallMembership is the active speaker now. Otherwise, false.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public bool IsActiveSpeaker
        {
            get
            {
                return (currentCall?.ActiveSpeaker?.PersonId == this.PersonId);
            }
        }

        internal bool IsSelf { get; set; }
        internal List<Device> Devices { get; set; }

        internal class Device
        {
            public string deviceType;
            public string reason;
            public string state;
            public string url;
        }
        private readonly Call currentCall;
        internal CallMembership() { }
        internal CallMembership(Call currentCall) : base()
        {
            this.currentCall = currentCall;
        }
    }
}
