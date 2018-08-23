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
    /// Person contents. 
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public class Person
    {
        /// <summary>
        /// The id of this person.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Id { get; set; }

        /// <summary>
        /// The emails of this person.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public List<string> Emails { get; set; }

        /// <summary>
        /// The display name of this person.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string DisplayName { get; set; }

        /// <summary>
        /// The avatar name of this person.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Avatar { get; set; }

        /// <summary>
        /// The time stamp that this person being created.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public DateTime Created { get; set; }

        /// <summary>
        /// The nick name of this person
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string NickName { get; set; }

        /// <summary>
        /// The nick first name of this person
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string FirstName { get; set; }

        /// <summary>
        /// The nick last name of this person
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string LastName { get; set; }

        /// <summary>
        /// The organization id of this person
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string OrgId { get; set; }

        /// <summary>
        /// The type of this person, default is "person"
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Type { get; set; }
    }
}
