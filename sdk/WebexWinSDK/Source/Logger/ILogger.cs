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
using SparkNet;

namespace WebexSDK
{
    /// <summary>
    /// A protocol for logging in the SDK.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public interface ILogger
    {
        /// <summary>
        /// log a message.
        /// </summary>
        /// <param name="msg">the mesage to be logged</param>
        /// <remarks>Since: 0.1.0</remarks>
        void Log(string msg);
    }

    /// <summary>
    /// The enumeration of log message level.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public enum LogLevel
    {
        /// <summary>
        /// this is a trace message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Trace = 1,
        /// <summary>
        /// this is a detail message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Detail = 2,
        /// <summary>
        /// this is a debug message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Debug = 3,
        /// <summary>
        /// this is an infomation message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Info = 4,
        /// <summary>
        /// this is a warning message.
        /// </summary>
        Warn = 5,
        /// <summary>
        /// this is an error message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Error = 6,
    }

}
