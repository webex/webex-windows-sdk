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
    /// The media options of a call.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class MediaOption
    {
        internal IntPtr? LocalViewPtr { get; set; }
        internal IntPtr? RemoteViewPtr { get; set; }
        internal IntPtr? RemoteShareViewPtr { get; set; }

        internal SparkNet.MediaOption MediaOptionType { get; set; }


        private MediaOption(IntPtr? localViewPtr, IntPtr? remoteViewPtr, IntPtr? remoteShareViewPtr, SparkNet.MediaOption type)
        {
            this.LocalViewPtr = localViewPtr;
            this.RemoteViewPtr = remoteViewPtr;
            this.RemoteShareViewPtr = remoteShareViewPtr;
            this.MediaOptionType = type;
        }
        internal bool HasVideo
        {
            get { return (MediaOptionType == SparkNet.MediaOption.All || MediaOptionType == SparkNet.MediaOption.VideoOnly || MediaOptionType == SparkNet.MediaOption.ScreenShareOnly); }
        }

        internal bool HasShare
        {
            get { return (MediaOptionType == SparkNet.MediaOption.All || MediaOptionType == SparkNet.MediaOption.ScreenShareOnly); }
        }
        /// <summary>
        /// Constructs an audio only media option.
        /// </summary>
        /// <returns>An instance of media option.</returns>
        /// <remarks>Since: 0.1.0</remarks>
        public static MediaOption AudioOnly()
        {
            return new MediaOption(null,null,null,SparkNet.MediaOption.AudioOnly);
        }
        /// <summary>
        /// Constructs an audio and video media option with optional view handles.
        /// The view handles can be set after video ready event.
        /// </summary>
        /// <param name="localViewPtr">The local video view handle.</param>
        /// <param name="remoteViewPtr">The remote video view handle.</param>
        /// <returns>An instance of media option.</returns>
        /// <remarks>Since: 0.1.0</remarks>
        public static MediaOption AudioVideo(IntPtr? localViewPtr=null, IntPtr? remoteViewPtr=null)
        {
            return new MediaOption(localViewPtr, remoteViewPtr, null, SparkNet.MediaOption.All);
        }
        /// <summary>
        /// Constructs an audio, video, and share media option with optional view handles.
        /// The view handles can be set after video ready event.
        /// </summary>
        /// <param name="localViewPtr">The local video view handle.</param>
        /// <param name="remoteViewPtr">The remote video view handle.</param>
        /// <param name="remoteShareViewPtr">The share view handle.</param>
        /// <returns>An instance of media option.</returns>
        /// <remarks>Since: 0.1.0</remarks>
        public static MediaOption AudioVideoShare(IntPtr? localViewPtr=null, IntPtr? remoteViewPtr=null, IntPtr? remoteShareViewPtr=null)
        {
            return new MediaOption(localViewPtr, remoteViewPtr, remoteShareViewPtr, SparkNet.MediaOption.All);
        }
    }
}
