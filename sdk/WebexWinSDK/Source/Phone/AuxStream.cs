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
    /// An AuxStream instance represents an auxiliary stream.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public class AuxStream
    {
        /// <summary>
        /// Gets the view handle.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public IntPtr Handle { get; internal set; }


        /// <summary>
        /// Update the auxiliary stream view. When the view size is changed, you may need to refresh the view.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public void RefreshView()
        {
            if (Track > TrackType.Unknown)
            {
                this.currentCall?.m_core_telephoneService.updateView(currentCall.CallId, Handle, Track);
            }
        }

        /// <summary>
        /// Close this auxiliary stream. Client can manually invoke this API to close stream or automatically close the last opened stream by SDK.<see cref="IMultiStreamObserver.OnAuxStreamUnAvailable"/>
        /// </summary>
        public void CloseAuxStream()
        {
            currentCall?.CloseAuxStream(Handle);
        }

        internal CallMembership person;
        /// <summary>
        /// Gets the person represented this auxiliary stream.
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
        /// Gets a value indicating whether [this auxiliary stream is sending video].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auxiliary stream is sending video]; otherwise, <c>false</c>.
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
        /// Gets or sets a value indicating whether [the auxiliary stream is receiving video].
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


        private Call.VideoDimensions auxStreamSize;
        /// <summary>
        /// Gets the auxiliary stream view dimensions (points) of this call.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public Call.VideoDimensions AuxStreamSize
        {
            get
            {
                uint width = 0;
                uint height = 0;
                if (currentCall != null && currentCall.m_core_telephoneService != null
                    && currentCall.m_core_telephoneService.getVideoSize(this.currentCall.CallId, Track, ref width, ref height))
                {
                    auxStreamSize.Width = width;
                    auxStreamSize.Height = height;
                    SdkLogger.Instance.Debug($"get remote track[{Track}] video view size: width[{width}] height[{height}]");
                }
                else
                {
                    SdkLogger.Instance.Error($"get remote track[{Track}] video view size error.");
                }
                return auxStreamSize;
            }
        }

        internal SparkNet.TrackType Track { get; set; }
        internal bool IsInUse { get; set; }
        private readonly Call currentCall;
        private AuxStream() { }
        internal AuxStream(Call currentCall)
            : base()
        {
            this.currentCall = currentCall;
            Handle = IntPtr.Zero;
        }
    }
}
