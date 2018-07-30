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
    /// A data type represents a Team at Cisco Webex cloud.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public class Team
    {
        /// <summary>
        /// The identifier of this team.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Id { get; set; }

        /// <summary>
        /// The name of this team
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Name { get; set; }

        /// <summary>
        /// The timestamp that this team being created.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public DateTime Created { get; set; }
    }
}
