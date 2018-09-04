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
    /// The auxiliary stream event.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public abstract class AuxStreamEvent : CallEvent
    {
        /// <summary>
        /// the auxiliary stream.
        /// </summary>
        protected AuxStream auxStream;
        /// <summary>
        /// Initializes a new instance of the <see cref="AuxStreamEvent"/> class.
        /// </summary>
        /// <param name="call">current call instance.</param>
        /// <param name="auxStream">the auxiliary stream instance.</param>
        /// <remarks>Since: 2.0.0</remarks>
        protected AuxStreamEvent(Call call, AuxStream auxStream)
            : base(call)
        {
            this.auxStream = auxStream;
        }

        /// <summary>
        /// Get the auxiliary stream instance.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public AuxStream AuxStream
        {
            get
            {
                return this.auxStream;
            }
        }
    }

    /// <summary>
    /// The person represented this auxiliary stream is changed.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public class AuxStreamPersonChangedEvent : AuxStreamEvent
    {
        /// <summary>
        /// The former person represented this auxiliary stream
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public CallMembership FromPerson { get; internal set; }
        /// <summary>
        /// The new person represented this auxiliary stream
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public CallMembership ToPerson { get; internal set; }

        internal AuxStreamPersonChangedEvent(CallMembership oldperson, CallMembership newperson, Call call, AuxStream remoteAuxVideo)
            : base(call, remoteAuxVideo)
        {
            FromPerson = oldperson;
            ToPerson = newperson;
        }
    }
    /// <summary>
    /// This might be triggered when the local party muted or unmuted the auxiliary stream.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    internal class ReceivingAuxStreamEvent : AuxStreamEvent
    {
        internal ReceivingAuxStreamEvent(Call call, AuxStream remoteAuxVideo)
            : base(call, remoteAuxVideo)
        {
        }
    }

    /// <summary>
    /// This might be triggered when the auxiliary stream view size is changed, and client can get the detail from the property <see cref="AuxStream.AuxStreamSize"/>
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public class AuxStreamSizeChangedEvent : AuxStreamEvent
    {
        internal AuxStreamSizeChangedEvent(Call call, AuxStream remoteAuxVideo)
            : base(call, remoteAuxVideo)
        {
        }
    }

    /// <summary>
    /// This might be triggered when the network is unstable or the represented person muted or unmuted his video, and client can get the detail from the property <see cref="AuxStream.IsSendingVideo"/>
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public class AuxStreamSendingVideoEvent : AuxStreamEvent
    {
        internal AuxStreamSendingVideoEvent(Call call, AuxStream remoteAuxVideo)
            : base(call, remoteAuxVideo)
        {
        }
    }

    /// <summary>
    /// This might be triggered when auxiliary stream is opened successfully or unsuccessfully.
    /// On this event, the client can display the view.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public class AuxStreamOpenedEvent : AuxStreamEvent
    {
        /// <summary>
        /// The result of opening the stream.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public WebexApiEventArgs<IntPtr> Result { get; internal set; }
        internal AuxStreamOpenedEvent(Call call, AuxStream remoteAuxVideo, WebexApiEventArgs<IntPtr> result)
            : base(call, remoteAuxVideo)
        {
            Result = result;
        }
    }

    /// <summary>
    /// This might be triggered when auxiliary stream is closed successfully or unsuccessfully.
    /// On this event, the client can hide the view.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public class AuxStreamClosedEvent : AuxStreamEvent
    {
        /// <summary>
        /// The result of closing the stream.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public WebexApiEventArgs<IntPtr> Result { get; internal set; }
        internal AuxStreamClosedEvent(Call call, WebexApiEventArgs<IntPtr> result)
            : base(call, null)
        {
            Result = result;
        }
    }
}
