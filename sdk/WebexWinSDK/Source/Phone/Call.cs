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
    /// A Call represents a media call on Cisco Webex.
    /// The application can create an outgoing call by calling phone.dial function.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class Call
    {
        internal string CallId { get; set; }
        internal string CalleeAddress { get; set; }
        internal bool IsUsed { get; set; }
        internal bool IsGroup { get; set; }
        internal bool IsSignallingConnected { get; set; }
        internal bool IsMediaConnected { get; set; }
        internal MediaOption MediaOption { get; set; }
        internal bool IsWaittingVideoCodecActivate { get; set; }
        internal event Action<WebexApiEventArgs> AnswerCompletedHandler;

        private readonly Phone phone;
        private readonly SparkNet.CoreFramework m_core;
        private readonly SparkNet.TelephonyService m_core_telephoneService;
        internal bool isSendingVideo;
        internal bool isSendingAudio;
        private bool isReceivingVideo;
        private bool isReceivingAudio;
        private bool isReceivingShare;
        private bool isRemoteSendingVideo;
        private bool isRemoteSendingAudio;
        private bool isRemoteSendingShare;
        internal bool isSendingShare;
        private VideoDimensions localVideoViewSize;
        private VideoDimensions remoteVideoViewSize;
        private VideoDimensions remoteShareViewSize;
        private CallStatus status;
        private CallDirection direction;
        private List<CallMembership> memberships;
        internal int JoinedCallMembershipCount = 0;
        internal int RemoteAuxVideoCount = 0;
        internal int RemoteAuxVideoAccurateCount = 0;



        internal Call(Phone phone)
        {
            this.phone = phone;
            Init();
            m_core = SCFCore.Instance.m_core;
            m_core_telephoneService = SCFCore.Instance.m_core_telephoneService;
        }

        internal void Init()
        {
            CallId = null;
            CalleeAddress = null;
            IsUsed = false;
            status = CallStatus.Disconnected;
            isSendingVideo = false;
            isSendingAudio = false;
            isReceivingVideo = true;
            isReceivingAudio = true;
            isRemoteSendingVideo = false;
            isRemoteSendingAudio = false;
            isReceivingShare = true;
            memberships = new List<CallMembership>();
            IsLocalRejectOrEndCall = false;
            IsGroup = false;
            IsWaittingVideoCodecActivate = false;
            RemoteAuxVideos = new List<RemoteAuxVideo>();
        }
        /// <summary>
        /// The enumeration of directions of a call
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public enum CallDirection
        {
            /// <summary>
            /// The local party is a recipient of the call.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            Incoming,

            /// <summary>
            /// The local party is an initiator of the call.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            Outgoing
        }

        /// <summary>
        /// video render view dimensions.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public struct VideoDimensions
        {
            /// <summary>
            /// the width of the video render view dimensions.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public uint Width { get; set; }
            /// <summary>
            /// the height of video render view dimensions.
            /// </summary>
            /// <remarks>Since: 0.1.0</remarks>
            public uint Height { get; set; }
        }

        /// <summary>
        /// Callback when remote participant(s) is ringing.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public event Action<Call> OnRinging;
        /// <summary>
        /// Callback when remote participant(s) answered and this call is connected.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public event Action<Call> OnConnected;
        /// <summary>
        /// Callback when this call is disconnected (hangup, cancelled, get declined or other self device pickup the call).
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public event Action<CallDisconnectedEvent> OnDisconnected;

        /// <summary>
        /// Callback when the memberships of this call have changed.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public event Action<CallMembershipChangedEvent> OnCallMembershipChanged;
        /// <summary>
        /// Callback when the media types of this call have changed.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public event Action<MediaChangedEvent> OnMediaChanged;

        /// <summary>
        /// Callback when the capabilities of this call have changed.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public event Action<Capabilities> OnCapabilitiesChanged;


        private event Action<WebexApiEventArgs<List<ShareSource>>> SelectShareSourceCompletedHandler = null;
        private event Action<WebexApiEventArgs<List<ShareSource>>> SelectAppShareSourceCompletedHandler = null;

        /// <summary>
        /// Gets the status of this call.
        /// </summary>
        /// <value>
        /// The status. <see cref="CallStatus"/>
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public CallStatus Status
        {
            get { return this.status; }
            internal set
            {
                SdkLogger.Instance.Info($"status change: {Status} -> {value}");
                this.status = value;
            }
        }


        /// <summary>
        /// Gets the direction of this call.
        /// </summary>
        /// <value>
        /// The direction. <see cref="CallDirection"/>
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public CallDirection Direction
        {
            get { return this.direction; }
            internal set
            {
                SdkLogger.Instance.Info($"call direction is {value.ToString()}");
                this.direction = value;
            }
        }

        internal CallDisconnectedEvent ReleaseReason { get; set; }

        internal bool IsLocalRejectOrEndCall { get; set; }


        /// <summary>
        /// Gets a value indicating whether [sending DTMF enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sending DTMF enabled]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsSendingDTMFEnabled
        {
            get
            {
                if (CallId == null)
                {
                    SdkLogger.Instance.Error("CallId is null.");
                    return false;
                }
                return m_core_telephoneService.canSendDTMF(CallId);
            }
        }

        /// <summary>
        /// Gets a value indicating whether [remote party of this call is sending video].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remote sending video]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsRemoteSendingVideo
        {
            get
            {
                return isRemoteSendingVideo;
            }
            internal set { isRemoteSendingVideo = value; }
        }

        /// <summary>
        /// Gets a value indicating whether [remote party of this call is sending audio].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remote sending audio]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsRemoteSendingAudio
        {
            get
            {
                return isRemoteSendingAudio;
            }
            internal set { isRemoteSendingAudio = value; }
        }

        /// <summary>
        /// True if the remote party of this call is sending share. Otherwise, false.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsRemoteSendingShare
        {
            get
            {
                return isRemoteSendingShare;
            }
            private set { isRemoteSendingShare = value; }
        }

        /// <summary>
        /// True if the local party of this call is sending share. Otherwise, false.
        /// </summary>
        /// <remarks>Since: 0.1.7</remarks>
        public bool IsSendingShare
        {
            get
            {
                return isSendingShare;
            }
            private set { isSendingShare = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [the local party of this call is sending video].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sending video]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsSendingVideo
        {
            get
            {
                return isSendingVideo;
            }
            set
            {
                SdkLogger.Instance.Info($"{value}");
                m_core_telephoneService?.muteVideo(CallId, !value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [the local party of this call is sending audio].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sending audio]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsSendingAudio
        {
            get
            {
                return isSendingAudio;
            }
            set
            {
                SdkLogger.Instance.Info($"{value}");
                m_core_telephoneService?.muteAudio(CallId, !value);
                isSendingAudio = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [the local party of this call is receiving video].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [receiving video]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsReceivingVideo
        {
            get
            {
                return isReceivingVideo;
            }
            set
            {
                SdkLogger.Instance.Info($"{value}");
                m_core_telephoneService?.muteRemoteVideo(CallId, !value, TrackType.Remote);
                isReceivingVideo = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [the local party of this call is receiving audio].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [receiving audio]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsReceivingAudio
        {
            get
            {
                return isReceivingAudio;
            }
            set
            {
                SdkLogger.Instance.Info($"{value}");
                m_core_telephoneService?.muteRemoteAudio(CallId, !value);
                isReceivingAudio = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [the local party of this call is receiving share].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [receiving  share]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public bool IsReceivingShare
        {
            get
            {
                return isReceivingShare;
            }
            set
            {
                SdkLogger.Instance.Info($"{value}");
                m_core_telephoneService?.muteRemoteVideo(CallId, !value, TrackType.RemoteShare);
                isReceivingShare = value;
            }
        }

        /// <summary>
        /// The local video render view dimensions (points) of this call.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public VideoDimensions LocalVideoViewSize
        {
            get
            {
                uint width = 0;
                uint height = 0;
                if (m_core_telephoneService != null && m_core_telephoneService.getVideoSize(CallId, TrackType.Local, ref width, ref height))
                {
                    localVideoViewSize.Width = width;
                    localVideoViewSize.Height = height;
                    SdkLogger.Instance.Debug($"get local video view size: width[{width}] height[{height}]");
                }
                else
                {
                    SdkLogger.Instance.Error("get local video view size error.");
                }
                return localVideoViewSize;
            }
        }

        /// <summary>
        /// The remote video render view dimensions (points) of this call.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public VideoDimensions RemoteVideoViewSize
        {
            get
            {
                uint width = 0;
                uint height = 0;
                if (m_core_telephoneService != null && m_core_telephoneService.getVideoSize(CallId, TrackType.Remote, ref width, ref height))
                {
                    remoteVideoViewSize.Width = width;
                    remoteVideoViewSize.Height = height;
                    SdkLogger.Instance.Debug($"get remote video view size: width[{width}] height[{height}]");
                }
                else
                {
                    SdkLogger.Instance.Error("get remote video view size error.");
                }
                return remoteVideoViewSize;
            }
        }

        /// <summary>
        /// The remote share render view dimensions (points) of this call.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public VideoDimensions RemoteShareViewSize
        {
            get
            {
                uint width = 0;
                uint height = 0;
                if (m_core_telephoneService != null && m_core_telephoneService.getVideoSize(CallId, TrackType.RemoteShare, ref width, ref height))
                {
                    remoteShareViewSize.Width = width;
                    remoteShareViewSize.Height = height;
                    SdkLogger.Instance.Debug($"get remote share view size: width[{width}] height[{height}]");
                }
                else
                {
                    SdkLogger.Instance.Error("get remote  share view size error.");
                }
                return remoteShareViewSize;
            }
        }
        internal CallMembership activeSpeaker;
        /// <summary>
        /// Gets the acitve speaker in this call. It would be changed dynamically in the meeting.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public CallMembership ActiveSpeaker
        {
            get
            {
                return this.activeSpeaker;
            }
        }

        /// <summary>
        /// Gets the memberships represent participants in this call.
        /// </summary>
        /// <value>
        /// The memberships.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public List<CallMembership> Memberships
        {
            get { return memberships; }
            internal set { memberships = value; }
        }

        /// <summary>
        /// Gets the initiator of this call.
        /// </summary>
        /// <value>
        /// The membership.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public CallMembership From
        {
            get
            {
                return Memberships.Find(item =>
                {
                    return item.IsInitiator;
                });
            }
        }

        /// <summary>
        /// Get the intended recipient of this call when one on one call.
        /// </summary>
        /// <value>
        /// The membership.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public CallMembership To {
            get
            {
                if (!IsGroup)
                {
                    return Memberships.Find(item =>
                    {
                        return !item.IsInitiator;
                    });
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Acknowledge (without answering) an incoming call.
        /// Will cause the initiator's Call instance to emit the ringing event.
        /// </summary>
        /// <param name="completedHandler">The completed event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Acknowledge(Action<WebexApiEventArgs> completedHandler)
        {
            SdkLogger.Instance.Info($"[{CallId}]");
            //scf auto return back an acknowledge message to caller.
            completedHandler?.Invoke(new WebexApiEventArgs(true, null));
        }


        /// <summary>
        /// Answers this call.
        /// This can only be invoked when this call is incoming.
        /// </summary>
        /// <param name="option">Intended media options - audio only or audio and video - for the call.</param>
        /// <param name="completedHandler">The completed event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Answer(MediaOption option, Action<WebexApiEventArgs> completedHandler)
        {
            if (Direction != CallDirection.Incoming)
            {
                SdkLogger.Instance.Error($"[{CallId}]: Failure: Unsupport function for outgoing call.");
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.IllegalOperation, "Unsupport function for outgoing call")));
            }

            if (Status > CallStatus.Ringing)
            {
                SdkLogger.Instance.Error($"[{CallId}]: Already connected, status:{Status.ToString()}");
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.IllegalStatus, "Already connected")));
            }
            SdkLogger.Instance.Info($"[{CallId}]: mediaOption:{option.MediaOptionType.ToString()}");

            MediaOption = option;

            // when video call, check if already activate the video codec license.
            // if already activated, continue call, otherwise, notify user to activate and wait the result
            if (!phone.CheckVideoCodecLicenseActivation(option))
            {
                IsWaittingVideoCodecActivate = true;
                AnswerCompletedHandler = completedHandler;
                SdkLogger.Instance.Info("video codec license hasn't activated.");

                phone.TriggerOnRequestVideoCodecActivation();
                return;
            }

            m_core_telephoneService.setMediaOption(CallId, option.MediaOptionType);
            m_core_telephoneService.setAudioMaxBandwidth(CallId, phone.AudioMaxBandwidth);
            m_core_telephoneService.setVideoMaxBandwidth(CallId, phone.VideoMaxBandwidth);
            m_core_telephoneService.setScreenShareMaxBandwidth(CallId, phone.ShareMaxBandwidth);
            m_core_telephoneService?.joinCall(this.CallId);

            completedHandler?.Invoke(new WebexApiEventArgs(true, null));
        }



        /// <summary>
        /// Rejects this call. 
        /// This can only be invoked when this call is incoming and in ringing status.
        /// </summary>
        /// <param name="completedHandler">The completed event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Reject(Action<WebexApiEventArgs> completedHandler)
        {
            if (Direction != CallDirection.Incoming)
            {
                SdkLogger.Instance.Error($"[{CallId}]: Unsupport function for outgoing call");
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.IllegalOperation, "Unsupport function for outgoing call")));
            }

            if (Status > CallStatus.Ringing)
            {
                SdkLogger.Instance.Error($"[{CallId}]: Already connected");
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.IllegalStatus, "Already connected")));
            }

            SdkLogger.Instance.Info($"{CallId}");
            m_core_telephoneService?.declineCall(this.CallId);

            IsLocalRejectOrEndCall = true;
            completedHandler?.Invoke(new WebexApiEventArgs(true, null));
        }

        /// <summary>
        /// Disconnects this call.
        /// This can only be invoked when this call is in answered status.
        /// </summary>
        /// <param name="completedHandler">The completed event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Hangup(Action<WebexApiEventArgs> completedHandler)
        {
            if (Status == CallStatus.Disconnected)
            {
                SdkLogger.Instance.Error($"[{CallId}]: Already disconnected");
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.IllegalStatus, "Already disconnected")));
            }

            SdkLogger.Instance.Info($"{CallId}");
            m_core_telephoneService.endCall(this.CallId);

            IsLocalRejectOrEndCall = true;
            completedHandler?.Invoke(new WebexApiEventArgs(true, null));
        }

        /// <summary>
        /// Sends feedback for this call to Cisco Webex team.
        /// </summary>
        /// <param name="rating">The rating of the quality of this call between 1 and 5 where 5 means excellent quality.</param>
        /// <param name="comments">The comments for this call.</param>
        /// <param name="includeLogs">if set to <c>true</c> [include logs], default is false.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void SendFeedbackWith(int rating, string comments = null, bool includeLogs = false)
        {
            SdkLogger.Instance.Debug($"rating[{rating}], comments[{comments}], includeLogs[{includeLogs}]");
            m_core.sendRating(rating, comments, includeLogs);
        }

        /// <summary>
        /// Sends DTMF events to the remote party. Valid DTMF events are 0-9, *, #, a-d, and A-D.
        /// </summary>
        /// <param name="dtmf">any combination of valid DTMF events matching regex mattern "^[0-9#\*abcdABCD]+$"</param>
        /// <param name="completedHandler">The completed event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void SendDtmf(string dtmf, Action<WebexApiEventArgs> completedHandler)
        {
            string strValidDtmf = "";
            if (!IsSendingDTMFEnabled)
            {
                SdkLogger.Instance.Info($"this call[{CallId}] is not support dtmf");
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.UnsupportedDTMF,"")));
                return;
            }
            SdkLogger.Instance.Info($"{CallId}");
            strValidDtmf = m_core_telephoneService.sendDTMF(this.CallId, dtmf);
            if (strValidDtmf != dtmf)
            {
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.InvalidDTMF,"")));
                return;
            }
            completedHandler?.Invoke(new WebexApiEventArgs(true, null));
        }

        /// <summary>
        /// Set remote video to display
        /// </summary>
        /// <param name="handle"> the video dispaly window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void SetRemoteView(IntPtr handle)
        {
            SdkLogger.Instance.Debug($"handle:{handle}");
            m_core_telephoneService.setView(this.CallId, handle, TrackType.Remote);
        }

        /// <summary>
        /// Set local view to display.
        /// </summary>
        /// <param name="handle">the local video dispaly window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void SetLocalView(IntPtr handle)
        {
            SdkLogger.Instance.Debug($"handle:{handle}");
            m_core_telephoneService.setView(this.CallId, handle, TrackType.Local);
        }

        /// <summary>
        /// Set remote share view to display.
        /// </summary>
        /// <param name="handle">the remote share dispaly window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void SetRemoteShareView(IntPtr handle)
        {
            SdkLogger.Instance.Debug($"handle:{handle}");
            m_core_telephoneService.setView(this.CallId, handle, TrackType.RemoteShare);
        }

        /// <summary>
        /// Update remote video to display when video window is resized.
        /// </summary>
        /// <param name="handle"> the video dispaly window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void UpdateRemoteView(IntPtr handle)
        {
            SdkLogger.Instance.Debug($"handle:{handle}");
            m_core_telephoneService.updateView(this.CallId, handle, TrackType.Remote);
        }

        /// <summary>
        /// Update local view to display when video window is resized.
        /// </summary>
        /// <param name="handle">the local video dispaly window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void UpdateLocalView(IntPtr handle)
        {
            SdkLogger.Instance.Debug($"handle:{handle}");
            m_core_telephoneService.updateView(this.CallId, handle, TrackType.Local);
        }

        /// <summary>
        /// Update remote share view to display when video window is resized.
        /// </summary>
        /// <param name="handle">the remote share dispaly window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void UpdateRemoteShareView(IntPtr handle)
        {
            SdkLogger.Instance.Debug($"handle:{handle}");
            m_core_telephoneService.updateView(this.CallId, handle, TrackType.RemoteShare);
        }

        /// <summary>
        /// Gets the list of RemoteAuxVideo which has been subscribed.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public List<RemoteAuxVideo> RemoteAuxVideos { get; internal set; }

        /// <summary>
        /// Subscribe a new remote auxiliary video with a view handle. The Maximum of auxiliary videos you can subscribe is 4 currently.
        /// You can invoke this API When receive RemoteAuxVideosCountChangedEvent event or call status is connected.
        /// </summary>
        /// <param name="handle">the remote auxiliary dispaly window handle</param>
        /// <returns>The subscribed remote auxiliary video instance. Returen null if subscribing failed.</returns>
        /// <remarks>Since: 2.0.0</remarks>
        public RemoteAuxVideo SubscribeRemoteAuxVideo(IntPtr handle)
        {
            if (!IsGroup)
            {
                SdkLogger.Instance.Error("one2one call cannot subscribe remote auxiliary video.");
                return null;
            }
            if (RemoteAuxVideoAccurateCount == 0 && Status != CallStatus.Connected)
            {
                SdkLogger.Instance.Error("You can invoke this API When receive RemoteAuxVideosCountChangedEvent event.");
                return null;
            }
            if (RemoteAuxVideos.Count >= 4)
            {
                SdkLogger.Instance.Error("max count of remote auxiliary view is 4");
                return null;
            }
            m_core_telephoneService.subscribeAuxVideo(this.CallId);

            var newRemoteAuxView = new RemoteAuxVideo(this);
            newRemoteAuxView.AddViewHandle(handle);
            RemoteAuxVideos.Add(newRemoteAuxView);
            return newRemoteAuxView;
        }

        /// <summary>
        /// Unsubscribe the indicated remote auxiliary video.
        /// </summary>
        /// <param name="remoteAuxVideo"> The indicated remote auxiliary video.</param>
        /// <remarks>Since: 2.0.0</remarks>
        public void UnsubscribeRemoteAuxVideo(RemoteAuxVideo remoteAuxVideo)
        {
            if (remoteAuxVideo == null)
            {
                SdkLogger.Instance.Error($"input parameter invalid. remoteAuxVideo is null.");
                return;
            }
            RemoteAuxVideos.Remove(remoteAuxVideo);

            SdkLogger.Instance.Error($"unsubscribe track[{remoteAuxVideo?.Track}]");
            if (remoteAuxVideo.Track >= TrackType.RemoteAux1 && remoteAuxVideo.Track <= TrackType.RemoteAux4)
            {
                m_core_telephoneService.unSubscribeAuxVideo(this.CallId, remoteAuxVideo.Track);
            }
        }


        /// <summary>
        /// Fetch enumerated sources with a kind of source type
        /// </summary>
        /// <param name="sourceType">share source type.</param>
        /// <param name="completedHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.7</remarks>
        public void FetchShareSources(ShareSourceType sourceType, Action<WebexApiEventArgs<List<ShareSource>>> completedHandler)
        {
            if (Status != CallStatus.Connected)
            {
                completedHandler(new WebexApiEventArgs<List<ShareSource>>(false, new WebexError(WebexErrorCode.IllegalOperation, "call status is not connected."), null));
                return;
            }
            SdkLogger.Instance.Debug($"selected source type is {sourceType.ToString()}");
            if (sourceType == ShareSourceType.Application)
            {
                SelectAppShareSourceCompletedHandler = completedHandler;
            }
            else
            {
                SelectShareSourceCompletedHandler = completedHandler;
            }
            
            m_core_telephoneService.enumerateShareSources((SparkNet.ShareSourceType)sourceType);
        }

        /// <summary>
        /// Start share .
        /// </summary>
        /// <param name="sourceId">the selected share sourceId</param>
        /// <param name="completedHandler">The completed event handler.</param>
        /// <remarks>Since: 0.1.7</remarks>
        public void StartShare(string sourceId, Action<WebexApiEventArgs> completedHandler)
        {
            if (Status != CallStatus.Connected)
            {
                SdkLogger.Instance.Error("call status is not connected.");
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.IllegalOperation, "call status is not connected.")));
                return;
            }

            if (sourceId == null)
            {
                SdkLogger.Instance.Error("source is null or source id is null");
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.IllegalOperation, "share soure is invalid.")));
                return;
            }
            SdkLogger.Instance.Debug($"{sourceId}");
            m_core_telephoneService.startShare(this.CallId, sourceId);
            completedHandler?.Invoke(new WebexApiEventArgs(true, null));
        }

        /// <summary>
        /// Stop share .
        /// </summary>
        /// <param name="completedHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.7</remarks>
        public void StopShare(Action<WebexApiEventArgs> completedHandler)
        {
            if (Status != CallStatus.Connected)
            {
                SdkLogger.Instance.Error("call status is not connected.");
                completedHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.IllegalOperation, "call status is not connected.")));
                return;
            }
            SdkLogger.Instance.Debug("");
            m_core_telephoneService.stopShare(this.CallId);

            completedHandler?.Invoke(new WebexApiEventArgs(true,null));
        }
        internal void TrigerAnswerCompletedHandler(WebexApiEventArgs completedHandler)
        {
            AnswerCompletedHandler?.Invoke(completedHandler);
        }

        internal void TrigerOnRing()
        {
            OnRinging?.Invoke(this);
        }

        internal void TrigerOnConnected()
        {
            OnConnected?.Invoke(this);
        }

        internal void TrigerOnDisconnected(CallDisconnectedEvent reason)
        {
            OnDisconnected?.Invoke(reason);
        }

        internal void TrigerOnCapabiltyChanged(Capabilities capablity)
        {
            OnCapabilitiesChanged?.Invoke(capablity);
        }

        internal void TrigerOnMediaChanged(MediaChangedEvent mediaChangedEvent)
        {
            SdkLogger.Instance.Debug($"trigerOnMediaChanged: {mediaChangedEvent.GetType().Name}");

            if (mediaChangedEvent is ReceivingVideoEvent receivingVideoEvent)
            {
                isReceivingVideo = receivingVideoEvent.IsReceiving;
            }
            else if (mediaChangedEvent is ReceivingAudioEvent receivingAudioEvent)
            {
                isReceivingAudio = receivingAudioEvent.IsReceiving;
            }
            else if (mediaChangedEvent is SendingVideoEvent sendingVideoEvent)
            {
                isSendingVideo = sendingVideoEvent.IsSending;
            }
            else if (mediaChangedEvent is SendingAudioEvent sendingAudioEvent)
            {
                isSendingAudio = sendingAudioEvent.IsSending;
            }
            else if (mediaChangedEvent is RemoteSendingAudioEvent remoteSendingAudioEvent)
            {
                IsRemoteSendingAudio = remoteSendingAudioEvent.IsSending;
            }
            else if (mediaChangedEvent is RemoteSendingVideoEvent remoteSendingVideoEvent)
            {
                IsRemoteSendingVideo = remoteSendingVideoEvent.IsSending;
            }
            else if (mediaChangedEvent is RemoteSendingShareEvent remoteSendingShareEvent)
            {
                IsRemoteSendingShare = remoteSendingShareEvent.IsSending;
                isReceivingShare = IsRemoteSendingShare;
            }
            else if (mediaChangedEvent is ReceivingShareEvent receivingShareEvent)
            {
                isReceivingShare = receivingShareEvent.IsReceiving;
            }
            else if (mediaChangedEvent is SendingShareEvent sendingShareEvent)
            {
                IsSendingShare = sendingShareEvent.IsSending;
            }

            OnMediaChanged?.Invoke(mediaChangedEvent);
        }

        internal void TrigerOnCallMembershipChanged(CallMembershipChangedEvent callMembershipEvent)
        {
            SdkLogger.Instance.Info($"event[{callMembershipEvent.GetType().Name}] callmerbship[{callMembershipEvent.CallMembership.Email}]");
            if (callMembershipEvent is CallMembershipLeftEvent)
            {
                var leftperson = callMembershipEvent as CallMembershipLeftEvent;
                CheckAuxVideoPersonChange(leftperson.CallMembership);
            }
            OnCallMembershipChanged?.Invoke(callMembershipEvent);
        }
        private void CheckAuxVideoPersonChange(CallMembership leftPerson)
        {
            if (RemoteAuxVideos != null && RemoteAuxVideos.Count > 0)
            {
                foreach (var item in RemoteAuxVideos)
                {
                    if (item.IsInUse && item.Person.PersonId == leftPerson.PersonId)
                    {
                        SdkLogger.Instance.Debug($"{item.Track} change to no person.");
                        var oldperson = item.Person;
                        item.person = null;
                        item.IsInUse = false;
                        TrigerOnMediaChanged(new RemoteAuxVideoPersonChangedEvent(oldperson, item.Person, this, item));
                    }
                }
            }
        }

        internal void TrigerOnSelectShareSource( ShareSourceType type)
        {
            var result = new List<ShareSource>();
            var shareSources = m_core_telephoneService.getShareSources((SparkNet.ShareSourceType)type);
            foreach (var item in shareSources)
            {
                result.Add(new ShareSource()
                {
                    SourceId = item.sourceId,
                    Name = item.name,
                });
            }
            if (type == ShareSourceType.Application)
            {
                SelectAppShareSourceCompletedHandler?.Invoke(new WebexApiEventArgs<List<ShareSource>>(true, null, result));
                SelectAppShareSourceCompletedHandler = null;
            }
            else
            {
                SelectShareSourceCompletedHandler?.Invoke(new WebexApiEventArgs<List<ShareSource>>(true, null, result));
                SelectShareSourceCompletedHandler = null;
            }
        }

        /// <summary>
        /// A RemoteAuxVideo instance represents a remote auxiliary video.
        /// </summary>
        /// <remarks>Since: 2.0.0</remarks>
        public class RemoteAuxVideo
        {
            /// <summary>
            /// Gets the list of view handle.
            /// </summary>
            /// <remarks>Since: 2.0.0</remarks>
            public List<IntPtr> HandleList { get; internal set; }

            /// <summary>
            /// Add a remote auxiliary video view.
            /// </summary>
            /// <param name="handle">The view handle.</param>
            public void AddViewHandle(IntPtr handle)
            {
                if (handle == IntPtr.Zero)
                {
                    return;
                }

                if (!HandleList.Contains(handle))
                {
                    HandleList.Add(handle);
                    if(Track > TrackType.Unknown)
                    {
                        this.currentCall?.m_core_telephoneService.setView(currentCall.CallId, handle, Track);
                    }
                }
            }
            /// <summary>
            /// Remove the remote auxiliary video view.
            /// </summary>
            /// <param name="handle">The view handle.</param>
            public void RemoveViewHandle(IntPtr handle)
            {
                if (handle == IntPtr.Zero)
                {
                    return;
                }

                if (HandleList.Contains(handle))
                {
                    HandleList.Remove(handle);
                    if (Track > TrackType.Unknown)
                    {
                        this.currentCall?.m_core_telephoneService.removeView(currentCall.CallId, handle, Track);
                    }
                }
            }

            /// <summary>
            /// Update the remote auxiliary video view.
            /// </summary>
            /// <param name="handle">The view handle.</param>
            public void UpdateViewHandle(IntPtr handle)
            {
                if (handle == IntPtr.Zero)
                {
                    return;
                }

                if (HandleList.Contains(handle) && Track > TrackType.Unknown)
                {
                    this.currentCall?.m_core_telephoneService.updateView(currentCall.CallId, handle, Track);
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
            public bool IsReceivingVideo
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


            private VideoDimensions remoteAuxVideoSize;
            /// <summary>
            /// Gets the remote auxiliary video view dimensions (points) of this call.
            /// </summary>
            /// <remarks>Since: 2.0.0</remarks>
            public VideoDimensions RemoteAuxVideoSize
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
                :base()
            {
                this.currentCall = currentCall;
                HandleList = new List<IntPtr>();
            }
        }
    }
}
