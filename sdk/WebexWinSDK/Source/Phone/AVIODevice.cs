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

namespace WebexSDK
{
    /// <summary>
    /// audio and video IO device type.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public enum AVIODeviceType
    {
        /// <summary>
        /// Invalid device.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Invalid = 0,
        /// <summary>
        /// This is microphone.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Microphone = 1,
        /// <summary>
        /// This is speaker.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Speaker = 2,
        /// <summary>
        /// This is camera.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Camera = 3,
        /// <summary>
        /// This is ringer.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        Ringer = 4
    }

    /// <summary>
    /// Audio and video IO device.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
#pragma warning disable S101 // Types should be named in camel case
    public class AVIODevice
#pragma warning restore S101 // Types should be named in camel case
    {
        /// <summary>
        /// If this is default device.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public bool DefaultDevice { get; set; }

        /// <summary>
        /// IO device ID.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Id { get; set; }

        /// <summary>
        /// IO device name.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Name { get; set; }

        /// <summary>
        /// IO device type.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public AVIODeviceType Type { get; set; }
    }
}
