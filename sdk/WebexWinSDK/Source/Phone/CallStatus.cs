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
    /// The status of a Call. 
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public enum CallStatus
    {
        /// <summary>
        /// For the outgoing call, the call has dialed.
        /// For the incoming call, the call has received.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Initiated,

        /// <summary>
        /// For the outgoing call, the call is ringing the remote party.
        /// For the incoming call, the call is ringing the local party.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Ringing,

        /// <summary>
        /// The call is answered.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Connected,

        /// <summary>
        /// The call is terminated.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Disconnected,
    }
}
