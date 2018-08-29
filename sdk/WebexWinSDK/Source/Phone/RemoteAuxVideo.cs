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
using SparkNet;

namespace WebexSDK
{
    /// <summary>
    /// A RemoteAuxVideo instance represents a remote auxiliary video.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public class RemoteAuxVideo
    {
        /// <summary>
        /// Gets the view handle.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public IntPtr Handle { get; internal set; }


        /// <summary>
        /// Update the remote auxiliary video view.
        /// </summary>
        /// <param name="handle">The view handle.</param>
        public void RefreshView()
        {
            if (Track > TrackType.Unknown)
            {
                this.currentCall?.m_core_telephoneService.updateView(currentCall.CallId, Handle, Track);
            }
        }
        internal CallMembership person;
        /// <summary>
        /// Gets the person represented this auxiliary video.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public CallMembership Person
        {
            get
            {
                return this.person;
            }
        }

        private bool isSendingVideo = false;
        /// <summary>
        /// Gets a value indicating whether [this remote auxiliary video is sending video].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remote auxiliary video is sending video]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 2.0.0</remarks>
        public bool IsSendingVideo
        {
            get
            {
                return this.isSendingVideo;
            }
            internal set
            {
                isSendingVideo = value;
            }
        }

        internal bool isReceivingVideo = true;
        /// <summary>
        /// Gets or sets a value indicating whether [the remote auxiliary video is receiving video].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [receiving video]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 2.0.0</remarks>
        internal bool IsReceivingVideo
        {
            get
            {
                return isReceivingVideo;
            }
            set
            {
                SdkLogger.Instance.Info($"{value}");
                this.currentCall.m_core_telephoneService?.muteRemoteVideo(this.currentCall.CallId, !value, Track);
                isReceivingVideo = value;
            }
        }


        private Call.VideoDimensions remoteAuxVideoSize;
        /// <summary>
        /// Gets the remote auxiliary video view dimensions (points) of this call.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public Call.VideoDimensions RemoteAuxVideoSize
        {
            get
            {
                uint width = 0;
                uint height = 0;
                if (currentCall != null && currentCall.m_core_telephoneService != null
                    && currentCall.m_core_telephoneService.getVideoSize(this.currentCall.CallId, Track, ref width, ref height))
                {
                    remoteAuxVideoSize.Width = width;
                    remoteAuxVideoSize.Height = height;
                    SdkLogger.Instance.Debug($"get remote track[{Track}] video view size: width[{width}] height[{height}]");
                }
                else
                {
                    SdkLogger.Instance.Error($"get remote track[{Track}] video view size error.");
                }
                return remoteAuxVideoSize;
            }
        }

        internal SparkNet.TrackType Track { get; set; }
        internal bool IsInUse { get; set; }
        private readonly Call currentCall;
        private RemoteAuxVideo() { }
        internal RemoteAuxVideo(Call currentCall)
            : base()
        {
            this.currentCall = currentCall;
            Handle = IntPtr.Zero;
        }
    }
}
