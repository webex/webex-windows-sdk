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
    /// The person represented this auxiliary video is changed.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public class AuxStreamPersonChangedEvent : AuxStreamEvent
    {
        /// <summary>
        /// The new person represented this auxiliary video
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public CallMembership FromPerson { get; internal set; }
        /// <summary>
        /// The former person represented this auxiliary video
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
    /// Remote auxiliary video view size has changed.
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
    /// This might be triggered when the network is unstable or the represented person muted or unmuted his video.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public class AuxStreamSendingEvent : AuxStreamEvent
    {
        internal AuxStreamSendingEvent(Call call, AuxStream remoteAuxVideo)
            : base(call, remoteAuxVideo)
        {
        }
    }

    public class AuxStreamOpenedEvent : AuxStreamEvent
    {
        public WebexApiEventArgs<IntPtr> Result { get; internal set; }
        internal AuxStreamOpenedEvent(Call call, AuxStream remoteAuxVideo, WebexApiEventArgs<IntPtr> result)
            : base(call, remoteAuxVideo)
        {
            Result = result;
        }
    }
    public class AuxStreamClosedEvent : AuxStreamEvent
    {
        public WebexApiEventArgs<IntPtr> Result { get; internal set; }
        internal AuxStreamClosedEvent(Call call, WebexApiEventArgs<IntPtr> result)
            : base(call, null)
        {
            Result = result;
        }
    }
}
