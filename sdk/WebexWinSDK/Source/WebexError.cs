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
    /// A data type represents a error.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public class WebexError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebexError"/> class.
        /// </summary>
        /// <param name="errorCode"><see cref="WebexErrorCode"/></param>
        /// <param name="reason">the detail reason of the error.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public WebexError(WebexErrorCode errorCode, string reason)
        {
            this.ErrorCode = errorCode;
            this.Reason = reason;
        }

        /// <summary>
        /// The error code <see cref="WebexErrorCode"/>
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public WebexErrorCode ErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// The reason of error.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Reason
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// The enumeration of error types.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public enum WebexErrorCode
    {

        /// <summary>
        /// A service request to Cisco Webex cloud has failed.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        ServiceFailed,
        /// <summary>
        /// The Phone has not been registered.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Unregistered,
        /// <summary>
        /// The media requires H.264 codec.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        RequireH264,
        /// <summary>
        /// The DTMF is invalid.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        InvalidDTMF,
        /// <summary>
        /// The DTMF is unsupported.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        UnsupportedDTMF,
        /// <summary>
        /// The service request is illegal.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        IllegalOperation,
        /// <summary>
        /// The service is in an illegal status.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        IllegalStatus,
    }
}
