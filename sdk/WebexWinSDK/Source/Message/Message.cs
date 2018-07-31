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
using System.Drawing;

namespace WebexSDK
{
    /// <summary>
    /// The struct of a Message on Cisco Webex.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public class Message
    {
        /// <summary>
        /// The identifier of this message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Id { get; set; }

        /// <summary>
        /// The identifier of the space where this message was posted.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string SpaceId { get; set; }

        /// <summary>
        /// The type of space, group or direct.
        /// </summary>
        public SpaceType SpaceType { get; set; }

        /// <summary>
        /// The identifier of the person who sent this message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string PersonId { get; set; }

        /// <summary>
        /// The email address of the person who sent this message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string PersonEmail { get; set; }

        /// <summary>
        /// The identifier of the recipient when sending a private 1:1 message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string ToPersonId { get; set; }

        /// <summary>
        /// The email address of the recipient when sending a private 1:1 message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string ToPersonEmail { get; set; }

        /// <summary>
        /// The timestamp that the message being created.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public DateTime Created { get; set; }

        /// <summary>
        /// If self is mentioned in this message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsSelfMentioned { get; set; }

        /// <summary>
        /// The content of the message text, it is plain text or rich text.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Text { get; set; }


        /// <summary>
        /// A array of public URLs of the attachments in the message.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public List<RemoteFile> Files { get; set; }

    }

    /// <summary>
    /// A data type represents a local file
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public class LocalFile
    {
        /// <summary>
        /// A data type represents a local file thumbnail.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public class Thumbnail
        {
            /// <summary>
            /// The path of the local file thumbnail.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public string Path { get; set; }
            /// <summary>
            /// The width of the thumbnail in pixels.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public int Width { get; set; }
            /// <summary>
            /// The height of the thumbnail in pixels.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public int Height { get; set; }
            /// <summary>
            /// The size of the thumbnail in bytes.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public UInt64 Size { get; set; }
            /// <summary>
            /// The mime type of the thumbnail.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public string Mime { get; set; }
        }

        /// <summary>
        /// The full path of the local file.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Path { get; set; }
        /// <summary>
        /// The display name of the local file.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Name { get; set; }
        /// <summary>
        /// The mime type of the file.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Mime { get; set; }
        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public UInt64 Size { get; set; }
        /// <summary>
        /// The delegation of the upload progress handler
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public Action<WebexApiEventArgs<int>> UploadProgressHandler { get; set; }
        /// <summary>
        /// The local file thumbnail.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public Thumbnail LocalThumbnail { get; set; }             
    }

    /// <summary>
    /// A data type represents a remote file
    /// </summary>
    public class RemoteFile
    {
        /// <summary>
        /// A data type represents a remote file thumbnail.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public class Thumbnail
        {
            /// <summary>
            /// The width of the thumbnail in pixels.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public int Width { get; set; }
            /// <summary>
            /// The height of the thumbnail in pixels.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public int Height { get; set; }
            /// <summary>
            /// The mime type of the thumbnail.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public string Mime { get; set; }
        }
        /// <summary>
        /// The display name of the remote file.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Name { get; set; }
        /// <summary>
        /// The mime type of the remote file.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public string Mime { get; set; }
        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public UInt64 Size { get; set; }
        /// <summary>
        /// The remote file thumbnail.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public Thumbnail RemoteThumbnail { get; set; }

        internal string SpaceId { get; set; }
        internal string MessageId { get; set; }
        internal int FileIndex { get; set; }
    }
    /// <summary>
    /// A abstact data type represents mention.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public abstract class Mention
    {
    }
    /// <summary>
    /// A data type represents mention one person in the space.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public class MentionPerson : Mention
    {
        private string personId;
        /// <summary>
        /// The constructor of the MentionPerson
        /// </summary>
        /// <param name="personId">The person ID.</param>
        public MentionPerson(string personId)
        {
            this.personId = personId;
        }
        /// <summary>
        /// Gets the persond id who is mentioned.
        /// </summary>
        public string PersonId
        {
            get { return this.personId; }
        }
    }
    /// <summary>
    /// A data type represents mention all in the space.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public class MentionAll : Mention
    {
    }
}


