
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
using System.Text.RegularExpressions;
using SparkNet;

namespace WebexSDK
{
    /// <summary>
    /// Phone represents a Cisco Webex calling device.
    /// The application can obtain a phone object from Webex object
    /// and use *phone* to call other Cisco Webex users or PSTN when enabled.
    /// The phone must be registered before it can make or receive calls.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class Phone
    {
        private bool isRegisteredToCore = false;
        readonly IAuthenticator authenticator;
        private static volatile Phone instance = null;
        private static readonly object lockHelper = new object();
        private readonly H264LicensePrompter prompter;

        private Phone(IAuthenticator authenticator)
        {
            this.authenticator = authenticator;
            this.currentCall = new Call(this);
            prompter = new H264LicensePrompter();
            isRegisteredToCore = false;
            isMercuryConnected = false;
            AudioMaxBandwidth = (UInt32)DefaultBandwidth.MaxBandwidthAudio;
            VideoMaxBandwidth = (UInt32)DefaultBandwidth.MaxBandwidth720p;
            ShareMaxBandwidth = (UInt32)DefaultBandwidth.MaxBandwidthSession;

            RegisterToCore();
        }

        private void RegisterToCore()
        {
            if (isRegisteredToCore)
            {
                return;
            }
            m_core = SCFCore.Instance.m_core;
            m_core_telephoneService = SCFCore.Instance.m_core_telephoneService;
            m_core_deviceManager = SCFCore.Instance.m_core_deviceManager;
            m_core.m_CallbackEvent += OnCoreCallBackPhone;

            isRegisteredToCore = true;
        }
        internal void UnRegisterToCore()
        {
            if (!isRegisteredToCore)
            {
                return;
            }
            m_core.m_CallbackEvent -= OnCoreCallBackPhone;
            m_core = null;
            m_core_telephoneService = null;
            m_core_deviceManager = null;
            isRegisteredToCore = false;
            DestoryInstance();
        }
        static void DestoryInstance()
        {
            instance = null;
        }

        internal static Phone GetInstance(IAuthenticator authenticator)
        {
            if (null == instance)
            {
                lock (lockHelper)
                {
                    if (null == instance)
                    {
                        instance = new Phone(authenticator);
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// The enumeration of common bandwidth choices.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public enum DefaultBandwidth
        {
            /// 177Kbps for 160x90 resolution
            MaxBandwidth90p = 177000,
            /// 384Kbps for 320x180 resolution
            MaxBandwidth180p = 384000,
            /// 768Kbps for 640x360 resolution
            MaxBandwidth360p = 768000,
            /// 2Mbps for 1280x720 resolution
            MaxBandwidth720p = 2000000,
            /// 3Mbps for 1920x1080 resolution
            MaxBandwidth1080p = 3000000,
            /// 4Mbps data session
            MaxBandwidthSession = 4000000,
            /// 64kbps for voice
            MaxBandwidthAudio = 64000,
        }

        /// <summary>
        /// The max bandwidth for audio in unit bps for the call.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public uint AudioMaxBandwidth { get; set; }

        /// <summary>
        /// The max bandwidth for video in unit bps for the call.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public uint VideoMaxBandwidth { get; set; }

        /// <summary>
        /// The max bandwidth for sharing in unit bps for the call.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public uint ShareMaxBandwidth { get; set; }

        /// <summary>
        /// Callback when call is incoming.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public event Action<Call> OnIncoming;

        /// <summary>
        /// Callback when request video codec activation
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public event Action OnRequestVideoCodecActivation;


        private Call currentCall;
        private bool isMercuryConnected;

        private SparkNet.CoreFramework m_core;
        private SparkNet.TelephonyService m_core_telephoneService;
        private SparkNet.DeviceManager m_core_deviceManager;

        private event Action<WebexApiEventArgs> RegisterCompletedHandler;
        private event Action<WebexApiEventArgs> DeregisterCompletedHandler;
        private event Action<WebexApiEventArgs<Call>> DialCompletedHandler;
        private event Action<bool> CameraPreviewReadyEvent;

        /// <summary>
        /// Registers this phone to Cisco Webex cloud on behalf of the authenticated user.
        /// It also creates the websocket and connects to Cisco Webex cloud.
        /// </summary>
        /// <param name="completedHandler">The completed event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Register(Action<WebexApiEventArgs> completedHandler)
        {
            SdkLogger.Instance.Debug("");

            if (m_core.getMercuryState() == MercuryState.Connected)
            {
                isMercuryConnected = true;
                completedHandler(new WebexApiEventArgs(true, null));
                return;
            }

            RegisterCompletedHandler = completedHandler;
            m_core.register2Mercury();        
        }


        /// <summary>
        /// Removes this phone from Cisco Webex cloud on behalf of the authenticated user.
        /// It also disconnects the websocket from Cisco Webex cloud.
        /// Subsequent invocations of this method behave as a no-op.
        /// </summary>
        /// <param name="completedHandler">The completed event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Deregister(Action<WebexApiEventArgs> completedHandler)
        {
            SdkLogger.Instance.Debug("");
            if(m_core.getMercuryState() == MercuryState.Disconnected)
            {
                isMercuryConnected = false;
                completedHandler(new WebexApiEventArgs(true, null));
                return;
            }

            isMercuryConnected = false;
            DeregisterCompletedHandler = completedHandler;
            m_core.disconnectFromMercury();         
        }

        /// <summary>
        /// Makes a call to an intended recipient on behalf of the authenticated user.
        /// </summary>
        /// <remarks>
        /// It supports the following address formats for the recipient:
        ///  * Webex URI: e.g. webex:shenning@cisco.com
        ///  * SIP / SIPS URI: e.g. sip:1234@care.acme.com
        ///  * Tropo URI: e.g. tropo:999123456
        ///  * Email address: e.g. shenning@cisco.com
        /// </remarks>
        /// <param name="address">Intended recipient address in one of the supported formats.</param>
        /// <param name="option">Intended media options - audio only or audio and video - for the call.</param>
        /// <param name="completedHandler">The completed event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Dial(string address, MediaOption option, Action<WebexApiEventArgs<Call>> completedHandler)
        {
            if (address == null || address.Length == 0)
            {
                SdkLogger.Instance.Error($"invalid parameter. address:{address}");
                completedHandler?.Invoke(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid address"), null));
                return;
            }

            if (option == null || completedHandler == null)
            {
                SdkLogger.Instance.Error($"invalid parameter. option or completedHandler is null");
                completedHandler?.Invoke(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.IllegalOperation, "option or completedHandler is null."), null));
                return;
            }

            if (!isMercuryConnected)
            {
                SdkLogger.Instance.Error("phone is not registered");
                completedHandler(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.Unregistered, "phone is not registered"), null));
                return;
            }

            if (currentCall.IsUsed)
            {
                SdkLogger.Instance.Error("Failure: There are other active calls");
                completedHandler(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.IllegalOperation, "There are other active calls"), currentCall));
                return;
            }

            SdkLogger.Instance.Debug($"callee address: {address}, media option: {option.MediaOptionType.ToString()}");

            currentCall.IsUsed = true;
            currentCall.Direction = Call.CallDirection.Outgoing;
            currentCall.Status = CallStatus.Initiated;
            currentCall.MediaOption = option;
            currentCall.CalleeAddress = address;
            DialCompletedHandler = completedHandler;

            // when video call, check if already activate the video codec license.
            // if already activated, continue call, otherwise, notify user to activate and wait the result
            if (!CheckVideoCodecLicenseActivation(option))
            {
                currentCall.IsWaittingVideoCodecActivate = true;
                SdkLogger.Instance.Info("video codec license hasn't activated.");

                TriggerOnRequestVideoCodecActivation();
                return;
            }

            ConvertToDialAddress(address, (isSpaceCall, outputAddress) =>
            {
                if (isSpaceCall)
                {
                    if (!m_core_telephoneService.canMakeCall(outputAddress))
                    {
                        currentCall = new Call(this);
                        SdkLogger.Instance.Error($"canMakeCall return false. address:{outputAddress}");
                        completedHandler?.Invoke(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.IllegalOperation, "maybe space id is invalid"), null));
                        return;
                    }
                    OnCallStarted(outputAddress);
                    SdkLogger.Instance.Debug($"This is a space call. join call: {outputAddress}");
                    m_core_telephoneService.joinCall(outputAddress);
                }
                else
                {
                    SdkLogger.Instance.Debug($"This is a direct call. make call: {outputAddress}");
                    m_core_telephoneService.makeCall(outputAddress);
                }
            });
        }

        internal void TriggerOnRequestVideoCodecActivation()
        {
            if (OnRequestVideoCodecActivation != null)
            {
                OnRequestVideoCodecActivation.Invoke();
            }
            else
            {
                ActivateVideoCodecLicense(false);
            }
            
        }

        internal bool CheckVideoCodecLicenseActivation(MediaOption option)
        {
            if (option.HasVideo && !prompter.Check())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Notify the end user to activate the use of H.264 codec license from Cisco Systems, Inc.
        /// Invoking this function is optional since the alert will appear automatically during the first video call.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public void RequestVideoCodecActivation()
        {
            if (!prompter.Check())
            {
                OnRequestVideoCodecActivation?.Invoke();
            }
        }

        /// <summary>
        /// Prevents Cisco Webex SDK from popping up an Alert for the end user to approve the use of H.264 video codec license from Cisco Systems, Inc.
        /// </summary>
        /// <param name="disable">True means disable otherwise false</param>
        /// - attention: The function is expected to be called only by Cisco internal applications. 3rd-party applications should NOT call this function.
        /// <remarks>Since: 0.1.0</remarks>
        public void DisableVideoCodecActivation(bool disable)
        {
            prompter.IsVideoLicenseActivationDisabled = disable;
        }

        /// <summary>
        /// Response to onRequestVideoCodecActivation event.
        /// </summary>
        /// <param name="activate">True means accept otherwise false.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void ActivateVideoCodecLicense(bool activate)
        {
            prompter.IsVideoLicenseActivated = activate;

            if (!currentCall.IsUsed || !currentCall.IsWaittingVideoCodecActivate)
            {
                return;
            }

            currentCall.IsWaittingVideoCodecActivate = false;

            //continue answer incoming call
            if (currentCall.Direction == Call.CallDirection.Incoming && currentCall.Status == CallStatus.Ringing)
            {
                ContinueAnswerIncomingCall(activate);
            }
            //continue call out
            else if (currentCall.Direction == Call.CallDirection.Outgoing && currentCall.Status == CallStatus.Initiated)
            {
                ContinueCallOut(activate);
            }
        }
        private void ContinueAnswerIncomingCall(bool activate)
        {
            if (currentCall == null)
            {
                return;
            }

            if (currentCall.Direction == Call.CallDirection.Incoming && currentCall.Status == CallStatus.Ringing)
            {
                if (!activate)
                {
                    SdkLogger.Instance.Warn("reject video codec license");
                    currentCall?.TrigerAnswerCompletedHandler(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.RequireH264, "")));
                    return;
                }
                if (currentCall.MediaOption != null)
                {
                    m_core_telephoneService.setMediaOption(currentCall.CallId, currentCall.MediaOption.MediaOptionType);
                }
                m_core_telephoneService.setAudioMaxBandwidth(currentCall.CallId, AudioMaxBandwidth);
                m_core_telephoneService.setVideoMaxBandwidth(currentCall.CallId, VideoMaxBandwidth);
                m_core_telephoneService.setScreenShareMaxBandwidth(currentCall.CallId, ShareMaxBandwidth);
                m_core_telephoneService?.joinCall(currentCall.CallId);

                currentCall?.TrigerAnswerCompletedHandler(new WebexApiEventArgs(true, null));
            }
        }
        private void ContinueCallOut(bool activate)
        {
            if (currentCall == null)
            {
                return;
            }
            if (currentCall.Direction == Call.CallDirection.Outgoing && currentCall.Status == CallStatus.Initiated)
            {
                if (!activate)
                {
                    SdkLogger.Instance.Warn("reject video codec license");
                    currentCall = new Call(this);
                    DialCompletedHandler?.Invoke(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.RequireH264, ""), null));
                    DialCompletedHandler = null;
                    return;
                }
                ConvertToDialAddress(currentCall.CalleeAddress, (isSpaceCall, outputAddress) =>
                {
                    if (isSpaceCall)
                    {
                        m_core_telephoneService.joinCall(outputAddress);
                    }
                    else
                    {
                        m_core_telephoneService.makeCall(outputAddress);
                    }
                });
            }
        }
        /// <summary>
        /// Return the text of the H.264 codec license from Cisco Systems, Inc.
        /// </summary>
        /// <returns>the text of the H.264 codec license</returns>
        /// <remarks>Since: 0.1.0</remarks>
        public string VideoCodecLicense
        {
            get
            {
                return prompter.License;
            } 
        }

        /// <summary>
        /// Return the URL of the H.264 codec license from Cisco Systems, Inc.
        /// </summary>
        /// <returns>the URL of the H.264 codec license</returns>
        /// <remarks>Since: 0.1.0</remarks>
        public string VideoCodecLicenseURL
        {
            get
            {
                return prompter.LicenseURL;
            }  
        }

        /// <summary>
        /// Render a preview of the local party before the call is answered.
        /// </summary>
        /// <param name="handle">The preview display window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void StartPreview(IntPtr handle)
        {
            SdkLogger.Instance.Debug($"handle: {handle}");
            StartCaptureDevice(SparkNet.MediaOption.All);
            CameraPreviewReadyEvent = (isReady) =>
            {
                if (isReady)
                {
                    m_core_deviceManager.startCameraPreview(handle);
                }
            };          
        }

        /// <summary>
        /// Stop rendering the preview of the local party.
        /// </summary>
        /// <param name="handle">The preview display window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void StopPreview(IntPtr handle)
        {
            SdkLogger.Instance.Debug($"handle: {handle}");
            m_core_deviceManager.removeCameraPreview(handle);
            StopCaptureDevice();
        }

        /// <summary>
        /// Update the preview when video window is resized.
        /// </summary>
        /// <param name="handle">The preview display window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void UpdatePreview(IntPtr handle)
        {
            SdkLogger.Instance.Debug($"handle: {handle}");
            m_core_deviceManager.updateCameraPreview(handle);
        }

        /// <summary>
        /// Get audio/video IO devices
        /// </summary>
        /// <param name="type">The type of audio/video IO device, such as microphone, speaker, camera, ringer. <see cref="AVIODeviceType"/></param>
        /// <returns>List of audio/video IO devices.</returns>
        /// <remarks>Since: 0.1.0</remarks>
        public List<WebexSDK.AVIODevice> GetAVIODevices(AVIODeviceType type)
        {
            SdkLogger.Instance.Debug($"get {type.ToString()} devices");
            SparkNet.Device[] devices = m_core_deviceManager.getDevices((SparkNet.DeviceType)type);
            var list = new List<WebexSDK.AVIODevice>();
            foreach (var d in devices)
            {
                WebexSDK.AVIODevice item = new WebexSDK.AVIODevice
                {
                    DefaultDevice = d.defaultDevice,
                    Id = d.id,
                    Name = d.name,
                    Type = (WebexSDK.AVIODeviceType)d.type
                };
                list.Add(item);
                d.Dispose();
            }   
            return list;
        }

        /// <summary>
        /// Select an audio/video IO device
        /// </summary>
        /// <param name="device">the selected audio/video IO device. <see cref="AVIODevice"/></param>
        /// <returns>The result of select.True means select success, otherwise false.</returns>
        /// <remarks>Since: 0.1.0</remarks>
        public bool SelectAVIODevice(AVIODevice device)
        {
            SdkLogger.Instance.Debug($"select {device.Name}");
            var item = m_core_deviceManager.getDevice(device.Id, (SparkNet.DeviceType)device.Type);
            bool result = m_core_deviceManager.selectDevice(item);
            if (result && item.type == SparkNet.DeviceType.Camera)
            {
                currentCall?.TrigerOnMediaChanged(new CameraSwitchedEvent(currentCall, device));
            }
            else if (result && item.type == SparkNet.DeviceType.Speaker)
            {
                currentCall?.TrigerOnMediaChanged(new SpeakerSwitchedEvent(currentCall, device));
            }
            return result;
        }

        private bool IsValidEmail(string address)
        {
            string strRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
            bool isEmail = Regex.IsMatch(address, strRegex, RegexOptions.IgnoreCase);
            return isEmail;
        }

        private bool IsJwtUserEmail(string address)
        {
            return (address.Contains("@") && !address.Contains("."));
        }

        private void ConvertToDialAddress(string address, Action<bool, string> completedHandler)
        {
            string outputAddress = address;
            bool isSpaceCall = false;

            //hydraID
            if (ParseHydraId(address, ref isSpaceCall, ref outputAddress))
            {
                completedHandler?.Invoke(isSpaceCall, outputAddress);
            }
            //email address
            else if (IsValidEmail(address))
            {
                outputAddress = address;
                completedHandler?.Invoke(isSpaceCall, outputAddress);
            }
            //jwt user email convert to user id
            else if (IsJwtUserEmail(address))
            {
                outputAddress = address;
                var webex = new Webex(this.authenticator);
                webex.People.List(address, null, 1, result =>
                {
                    if (result.IsSuccess)
                    {
                        List<Person> persons = result.Data;
                        if (persons.Count != 0)
                        {
                            ParseHydraId(persons[0].Id, ref isSpaceCall, ref outputAddress);
                        }

                        completedHandler?.Invoke(isSpaceCall, outputAddress);
                    }
                });
            }
            else
            {
                completedHandler?.Invoke(isSpaceCall, address);
            }
        }

        private bool ParseHydraId(string address, ref bool isSpace, ref string outputAddress)
        {
#pragma warning disable S1075 // URIs should not be hardcoded
            string peopleUrl = "ciscospark://us/PEOPLE/";
            string spaceUrl = "ciscospark://us/ROOM/";
#pragma warning restore S1075 // URIs should not be hardcoded

            isSpace = false;
            outputAddress = null;

            try
            {
                var decodedStr = StringExtention.Base64UrlDecode(address);
                if (decodedStr.StartsWith(peopleUrl))
                {
                    outputAddress = decodedStr.Substring(peopleUrl.Length);
                }
                else if (decodedStr.StartsWith(spaceUrl))
                {
                    outputAddress = decodedStr.Substring(spaceUrl.Length);
                    isSpace = true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }


            return true;
        }



        private void StartCaptureDevice(SparkNet.MediaOption option)
        {
            SdkLogger.Instance.Debug($"{option.ToString()}");
            m_core_deviceManager.startCaptureDevice(option);
        }
        private void StopCaptureDevice()
        {
            SdkLogger.Instance.Debug("");
            m_core_deviceManager.stopCaptureDevice();
        }


        private void OnCoreCallBackPhone(SCFEventType type, int error, string status)
        {
            SdkLogger.Instance.Debug($"event type:{type}, error[{error}], status:{status}");
            switch (type)
            {
                case SCFEventType.ParticipantsChanged:
                    OnParticipantsChanged(status);
                    break;
                case SCFEventType.MercuryStateChange:
                    OnMercuryStateChange((SparkNet.MercuryState)error);
                    break;
                case SCFEventType.JwtAccessTokenExpired:
                    OnJwtAccessTokenExpired();
                    break;
                case SCFEventType.CallStarted:
                    OnCallStarted(status);
                    break;
                case SCFEventType.StartRing:
                    OnStartRing((SparkNet.RingerType)error, status);
                    break;
                case SCFEventType.CallCreatedWithConveration:
                    currentCall.CallId = status;
                    break;
                case SCFEventType.CallDisconnected:
                    OnCallDisconnected(status);
                    break;
                case SCFEventType.CallConnected:
                    if(currentCall != null)
                    {
                        currentCall.IsMediaConnected = true;
                        OnCallConnected();
                    }
                    break;
                case SCFEventType.CallTerminated:
                case SCFEventType.CallFailed:
                    OnCallTerminated(status);
                    break;
                case SCFEventType.RemoteVideoReady:
                case SCFEventType.LocalVideoReady:
                    OnVideoReady(type, (SparkNet.TrackType)error);
                    break;
                case SCFEventType.RemoteVideoStop:
                    OnRemoteVideoStop((SparkNet.TrackType)error);
                    break;
                case SCFEventType.CallConversationChanged:
                    currentCall.CallId = m_core_telephoneService.getCurrentCallId();
                    break;
                case SCFEventType.ShowIncomingCallAlert:
                    OnShowIncomingCallAlert(error, status);
                    break;
                case SCFEventType.AudioMutedStateChanged:
                    OnAudioMutedStateChanged((SparkNet.TrackType)error, status);
                    break;
                case SCFEventType.MuteRemoteAudioDone:
                    OnMuteRemoteAudioDone((SparkNet.TrackType)error, status);
                    break;
                case SCFEventType.MuteRemoteVideoDone:
                    OnMuteRemoteVideoDone((SparkNet.TrackType)error, status);
                    break;
                case SCFEventType.VideoSizeChanged:
                    OnVideoSizeChanged((SparkNet.TrackType)error);
                    break;
                case SCFEventType.DTMFStatus:
                    OnDTMFStatusChanged((SparkNet.DTMFCapStatus)error);
                    break;
                case SCFEventType.CameraPreviewReady:
                    OnCameraPreviewReady();
                    break;
                case SCFEventType.LocalContentSharingStarted:
                case SCFEventType.LocalContentSharingStop:
                case SCFEventType.RemoteContentSharingStarted:
                case SCFEventType.RemoteContentSharingStop:
                case SCFEventType.EnumeratedShareSourcesCallback:
                case SCFEventType.EnumeratedAppShareSourcesCallback:
                    OnRemoteContentSharingStateChanged(type);
                    break;
                case SCFEventType.VideoTrackPersonChanged:
                    OnVideoTrackPersonChanged((TrackType)error, status);
                    break;
                case SCFEventType.IsAuxVideoStreamInUseChanged:
                    OnIsAuxVideoStreamInUseChanged((TrackType)error, status);
                    break;
                case SCFEventType.IsVideoStreamingChanged:
                    OnIsVideoStreamingChanged((TrackType)error, status);
                    break;
                case SCFEventType.IsAudioStreamingChanged:
                    OnIsAudioStreamingChanged((TrackType)error, status);
                    break;
                case SCFEventType.RemoteVideoCountChanged:
                    OnRemoteVideoCountChanged(error, status);
                    break;
                default:
                    break;
            }
        }

        private void OnVideoTrackPersonChanged(TrackType trackType, string callId)
        {
            SdkLogger.Instance.Debug($"trackType[{trackType}], callID[{callId}]");
            if (trackType == TrackType.Remote)
            {
                OnRemoteTrackPersonChanged(callId);
            }
            else if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                OnRemoteAuxTrackPersonChanged(trackType, callId);     
            }
        }
        private void OnRemoteTrackPersonChanged(string callId)
        {
            SdkLogger.Instance.Debug("active speaker changed");
            var oldperson = currentCall.ActiveSpeaker;

            string contactId = m_core_telephoneService.getContact(callId, TrackType.Remote);
            if (contactId == null || contactId.Length == 0)
            {
                SdkLogger.Instance.Error($"get contactID by Remote Track failed.");
                currentCall.activeSpeaker = null;
            }
            else
            {
                var trackPersonId = StringExtention.EncodeHydraId(StringExtention.HydraIdType.People, contactId);
                currentCall.activeSpeaker = currentCall?.Memberships?.Find(x => x.PersonId == trackPersonId);
            }
            if (currentCall.ActiveSpeaker != oldperson)
            {
                currentCall?.TrigerOnMediaChanged(new ActiveSpeakerChangedEvent(currentCall, currentCall.ActiveSpeaker, oldperson));
            }
        }
        private void OnRemoteAuxTrackPersonChanged(TrackType trackType, string callId)
        {
            var find = currentCall?.AuxStreams.Find(x => (x.Track == trackType));
            if (find != null)
            {
                SdkLogger.Instance.Debug($"{trackType} person changed");
                var oldperson = find.Person;

                string contactId = m_core_telephoneService.getContact(callId, trackType);
                if (contactId == null || contactId.Length == 0)
                {
                    SdkLogger.Instance.Debug($"trackType[{trackType}] has no person.");
                    find.person = null;
                }
                else
                {
                    var trackPersonId = StringExtention.EncodeHydraId(StringExtention.HydraIdType.People, contactId);
                    find.person = this.currentCall.Memberships.Find(x => x.PersonId == trackPersonId);
                }
                if (find.Person != oldperson)
                {
                    currentCall?.TrigerOnAuxStreamEvent(new AuxStreamPersonChangedEvent(oldperson, find.Person, currentCall, find));
                }
            }
        }
        private void OnIsAuxVideoStreamInUseChanged(TrackType trackType, string callId)
        {
            bool isInUse = m_core_telephoneService.getIsVideoTrackInUse(callId, trackType);
            
            if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.AuxStreams.Find(x => (x.Track == trackType));
                if (find != null && find.IsInUse != isInUse)
                {
                    find.IsInUse = isInUse;
                    SdkLogger.Instance.Debug($"callID[{callId}] trackType[{trackType}] InUse[{isInUse}]");
                }
            }
        }
        private void OnIsVideoStreamingChanged(TrackType trackType, string callId)
        {
            bool isStreaming = m_core_telephoneService.getIsVideoStreaming(callId, trackType);
            SdkLogger.Instance.Debug($"callID[{callId}] trackType[{trackType}] IsStreaming[{isStreaming}]");

            if (trackType == TrackType.Local)
            {
                if (currentCall != null && currentCall.IsSendingVideo != isStreaming)
                {
                    currentCall.isSendingVideo = isStreaming;
                    currentCall.TrigerOnMediaChanged(new SendingVideoEvent(currentCall, isStreaming));
                }
            }
            else if (trackType == TrackType.Remote)
            {
                if (currentCall != null && currentCall.IsRemoteSendingVideo != isStreaming)
                {
                    currentCall.IsRemoteSendingVideo = isStreaming;
                    currentCall.TrigerOnMediaChanged(new RemoteSendingVideoEvent(currentCall, isStreaming));
                }
            }
            else if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.AuxStreams.Find(x => (x.Track == trackType));
                if (find != null && find.IsSendingVideo != isStreaming)
                {
                    find.IsSendingVideo = isStreaming;
                    currentCall?.TrigerOnAuxStreamEvent(new AuxStreamSendingVideoEvent(currentCall, find));
                }
            }
        }
        private void OnIsAudioStreamingChanged(TrackType trackType, string callId)
        {
            SdkLogger.Instance.Debug($"callID[{callId}] trackType[{trackType}]");
            
            if (trackType == TrackType.Remote)
            {
                currentCall.IsRemoteSendingAudio = true;
                currentCall?.TrigerOnMediaChanged(new RemoteSendingAudioEvent(currentCall, true));
            }
        }
        private void OnRemoteVideoCountChanged(int count, string callId)
        {
            if (currentCall == null)
            {
                return;
            }
            SdkLogger.Instance.Debug($"callID[{callId}] count[{count}]");
            currentCall.AuxStreamCount = count;
            OnRemoteAuxVideosCountChanged();
        }

        private void OnRemoteAuxVideosCountChanged()
        {
            if (currentCall == null || currentCall.JoinedCallMembershipCount < 2)
            {
                return;
            }

            SdkLogger.Instance.Debug($"JoinedCallMembershipCount:{currentCall.JoinedCallMembershipCount} AuxStreamCount: {currentCall.AuxStreamCount}");
            int min = Math.Min(currentCall.JoinedCallMembershipCount-2, currentCall.AuxStreamCount);
            if (min == currentCall.AvailableAuxStreamCount || min > currentCall.OpenAuxStreamMaxCount)
            {
                return;
            }
            bool isIncrease = min > currentCall.AvailableAuxStreamCount;
            int count = isIncrease? (min - currentCall.AvailableAuxStreamCount):(currentCall.AvailableAuxStreamCount - min);
            currentCall.AvailableAuxStreamCount = min;
            SdkLogger.Instance.Debug($"AvailableAuxStreamCount: {currentCall.AvailableAuxStreamCount}");

            if(currentCall.MultiStreamObserver != null)
            {
                TriggerAuxStreamAvailabelOrUnAvailabe(isIncrease, count);
            }

        }
        private void TriggerAuxStreamAvailabelOrUnAvailabe(bool isIncrease, int count)
        {
            if (currentCall.MultiStreamObserver == null)
            {
                return;
            }
            if(currentCall.AvailableAuxStreamCount > currentCall.OpenAuxStreamMaxCount)
            {
                return;
            }
            if (isIncrease)
            {
                SdkLogger.Instance.Debug($"OnAuxStreamAvailable, increase:{count}");
                while (count > 0)
                {
                    var handle = currentCall.MultiStreamObserver.OnAuxStreamAvailable();
                    if (handle != IntPtr.Zero)
                    {
                        currentCall.OpenAuxStream(handle);
                    }
                    count--;
                }
            }
            else if(currentCall.AvailableAuxStreamCount < currentCall.OpenAuxStreamMaxCount)
            {
                SdkLogger.Instance.Debug($"OnAuxStreamUnAvailable, decrease:{count}");
                while (count > 0)
                {
                    var handle = currentCall.MultiStreamObserver.OnAuxStreamUnAvailable();
                    if (handle == IntPtr.Zero && currentCall.AvailableAuxStreamCount < currentCall.AuxStreams.Count)
                    {
                        handle = currentCall.AuxStreams[currentCall.AuxStreams.Count - 1].Handle;
                    }
                    currentCall.CloseAuxStream(handle);
                    count--;
                }
            }
        }

        private void OnParticipantsChanged(string callId)
        {
            List<CallMembership> tmpMemberships = new List<CallMembership>();

            if(currentCall == null || currentCall.CallId == null)
            {
                SdkLogger.Instance.Warn("currentCall.CallId is null");
                return;
            }

            if (callId != currentCall.CallId)
            {
                return;
            }

            SparkNet.CallParticipant[] participants = m_core_telephoneService.getCallParticipants(currentCall.CallId);
            foreach (var item in participants)
            {
                //filter
                if(item.email == null || item.email.Length == 0)
                {
                    continue;
                }

                var tmpCallMembership = new CallMembership(currentCall)
                {
                    IsInitiator = item.creator,
                    PersonId = StringExtention.EncodeHydraId(StringExtention.HydraIdType.People, item.contactId),
                    State = ConvertToCallMembershipStateEnum(item.state),
                    Email = item.email,
                    SipUrl = item.sipUrl,
                    PhoneNumber = item.phoneNumber,
                    IsSendingVideo = !item.localVideoMuted,
                    IsSendingAudio = !item.localAudioMuted,
                    IsSendingShare = item.bSharing,
                    IsSelf = item.bSelf,
                    Devices = new List<CallMembership.Device>(),
                };
                foreach(var device in item.devices)
                {
                    tmpCallMembership.Devices.Add(new CallMembership.Device()
                    {
                        deviceType = device.deviceType,
                        reason = device.reason,
                        state = device.state,
                        url = device.url,
                    });
                }

                tmpMemberships.Add(tmpCallMembership);
            }
            List<CallMembership> oldMemberships = new List<CallMembership>(currentCall.Memberships);
            currentCall.Memberships = tmpMemberships;
            CompareParticipants(oldMemberships, tmpMemberships);

        }
        private CallMembership.CallState ConvertToCallMembershipStateEnum(string strIn)
        {
            CallMembership.CallState result = CallMembership.CallState.Unknown;

            
            if (String.Compare(strIn, CallMembership.CallState.Unknown.ToString(), true) == 0)
            {
                result = CallMembership.CallState.Unknown;
            }
            else if (String.Compare(strIn, CallMembership.CallState.Idle.ToString(), true) == 0)
            {
                result = CallMembership.CallState.Idle;
            }
            else if (String.Compare(strIn, CallMembership.CallState.Joined.ToString(), true) == 0)
            {
                result = CallMembership.CallState.Joined;
            }
            else if (String.Compare(strIn, CallMembership.CallState.Left.ToString(), true) == 0)
            {
                result = CallMembership.CallState.Left;
            }
            else if (String.Compare(strIn, CallMembership.CallState.Notified.ToString(), true) == 0)
            {
                result = CallMembership.CallState.Notified;
            }
            else if (String.Compare(strIn, CallMembership.CallState.Declined.ToString(), true) == 0)
            {
                result = CallMembership.CallState.Declined;
            }

            return result;
        }

        private void CompareParticipants(List<CallMembership> oldMemberships, List<CallMembership> newMemberships)
        {
            foreach(var newItem in newMemberships)
            {
                var findMembership = oldMemberships.Find(i =>
                {
                    return i.PersonId == newItem.PersonId;
                });

                if (findMembership == null || findMembership.State != newItem.State)
                {
                    CompareParticipantsCallStateChanged(newItem);
                }
                if (findMembership == null)
                {
                    continue;
                }
                if (findMembership.IsSendingAudio != newItem.IsSendingAudio)
                {
                    currentCall?.TrigerOnCallMembershipChanged(new CallMembershipSendingAudioEvent(currentCall, newItem));
                }

                if (findMembership.IsSendingVideo != newItem.IsSendingVideo)
                {
                    currentCall?.TrigerOnCallMembershipChanged(new CallMembershipSendingVideoEvent(currentCall, newItem));
                }

                if (findMembership.IsSendingShare != newItem.IsSendingShare)
                {
                    currentCall?.TrigerOnCallMembershipChanged(new CallMembershipSendingShareEvent(currentCall, newItem));
                }
            }
        }
        private void CompareParticipantsCallStateChanged(CallMembership newItem)
        {
            if (currentCall == null)
            {
                return;
            }
            if (newItem.State == CallMembership.CallState.Joined)
            {
                if (!ProcessParticipantJoined(newItem))
                {
                    return;
                }
                currentCall.JoinedCallMembershipCount++;
                currentCall.TrigerOnCallMembershipChanged(new CallMembershipJoinedEvent(currentCall, newItem));
                if (currentCall.JoinedCallMembershipCount >= 3)
                {
                    OnRemoteAuxVideosCountChanged();
                }

            }
            else if (newItem.State == CallMembership.CallState.Declined)
            {
                currentCall?.TrigerOnCallMembershipChanged(new CallMembershipDeclinedEvent(currentCall, newItem));
            }
            else if (newItem.State == CallMembership.CallState.Left)
            {
                currentCall.JoinedCallMembershipCount = currentCall.Memberships.Count(x => x.State == CallMembership.CallState.Joined);
                currentCall.TrigerOnCallMembershipChanged(new CallMembershipLeftEvent(currentCall, newItem));
                if (currentCall.JoinedCallMembershipCount >= 2)
                {
                    OnRemoteAuxVideosCountChanged();
                }
            }
        }
        private bool ProcessParticipantJoined(CallMembership newItem)
        {
            if(currentCall==null)
            {
                SdkLogger.Instance.Error("currentCall is null.");
                return true;
            }

            //one2one outgoing call, trigger callConnected event when remote participant joined
            if (currentCall.IsOne2One && currentCall.Direction == Call.CallDirection.Outgoing)
            {
                if (!newItem.IsSelf)
                {
                    currentCall.IsSignallingConnected = true;
                    OnCallConnected();
                }
            }
            else
            {
                if (newItem.IsSelf)
                {
                    return ProcessSelfJoind(newItem);
                }
            }
            return true;
        }
        private bool ProcessSelfJoind(CallMembership newItem)
        {
            bool find = false;
            foreach (var device in newItem.Devices)
            {
                //self and this device join, trigger call connected
                if (m_core.getDeviceUrl() == device.url
                    && ConvertToCallMembershipStateEnum(device.state) == CallMembership.CallState.Joined)
                {
                    find = true;
                    currentCall.IsSignallingConnected = true;
                    OnCallConnected();
                    break;
                }
            }
            //self and other device join, trigger call release
            if (!find)
            {
                currentCall.ReleaseReason = new OtherConnected(currentCall);
                currentCall?.TrigerOnDisconnected(currentCall.ReleaseReason);
                currentCall = new Call(this);
                return false;
            }
            return true;
        }

        private void OnMercuryStateChange(SparkNet.MercuryState state)
        {
            SdkLogger.Instance.Info($"state: {state.ToString()}");
            if (state == SparkNet.MercuryState.Connected)
            {
                isMercuryConnected = true;
                RegisterCompletedHandler?.Invoke(new WebexApiEventArgs(true, null));
                RegisterCompletedHandler = null;
            }
            else if(state == SparkNet.MercuryState.Disconnected)
            {
                isMercuryConnected = false;
                DeregisterCompletedHandler?.Invoke(new WebexApiEventArgs(true, null));
                DeregisterCompletedHandler = null;

                // the mercury disconnected, maybe because the access token expired, so try to access token and get a new token if expired
                authenticator?.AccessToken(r=>
                {
                    if (!r.IsSuccess)
                    {
                        SdkLogger.Instance.Info("when mercury disconnected, access token fail.");
                    }
                });
            }

        }
        private void OnJwtAccessTokenExpired()
        {
            SdkLogger.Instance.Info("");
            authenticator?.RefreshToken(r =>
            {
                if (!r.IsSuccess)
                {
                    SdkLogger.Instance.Info("access token failed.");
                }
            });
        }
        private void OnCallStarted(string callId)
        {
            SdkLogger.Instance.Debug($"CallId[{callId}]");
            if (currentCall.CallId != null && currentCall.CallId != callId)
            {
                SdkLogger.Instance.Warn("already have a call");
                return;
            }
            currentCall.IsOne2One = m_core_telephoneService.getIsOne2One(currentCall.CallId);
            SdkLogger.Instance.Debug($"This is a {(currentCall.IsOne2One ? "One2One Call" : "meeting")}");

            // outgoing call
            if (currentCall.IsUsed && currentCall.Direction == Call.CallDirection.Outgoing)
            {
                currentCall.CallId = callId;
                m_core_telephoneService.setMediaOption(currentCall.CallId , currentCall.MediaOption.MediaOptionType);
                m_core_telephoneService.setAudioMaxBandwidth(currentCall.CallId, AudioMaxBandwidth);
                m_core_telephoneService.setVideoMaxBandwidth(currentCall.CallId, VideoMaxBandwidth);
                DialCompletedHandler?.Invoke(new WebexApiEventArgs<Call>(true, null, currentCall));
                DialCompletedHandler = null;
            }
        }

        private void OnShowIncomingCallAlert(int error, string callId)
        {
            // incoming call
            SdkLogger.Instance.Debug($"CallId[{callId}] error[{error}]");
            if (currentCall.CallId != null)
            {
                SdkLogger.Instance.Warn("already have a call, reject incoming call");
                m_core_telephoneService?.declineCall(callId);
                m_core_telephoneService?.endCall(callId);
                return;
            }

            currentCall.IsUsed = true;
            currentCall.Direction = Call.CallDirection.Incoming;
            currentCall.Status = CallStatus.Initiated;
            currentCall.CallId = callId;

            OnIncoming?.Invoke(currentCall);
        }

        private void OnStartRing(SparkNet.RingerType ringerType, string callId)
        {
            if (currentCall.Status >= CallStatus.Ringing)
            {
                SdkLogger.Instance.Warn($"current call state is already {currentCall.Status}, not process startring event");
                return;
            }
            SdkLogger.Instance.Info($"CallID[{callId}], ringer type is {ringerType.ToString()}");

            currentCall.Status = CallStatus.Ringing;
            currentCall?.TrigerOnRing();
        }

        private void OnCallConnected()
        {
            if (currentCall == null ||
                !currentCall.IsSignallingConnected || 
                !currentCall.IsMediaConnected)
            {
                return;
            }
            if (currentCall.Status >= CallStatus.Connected)
            {
                SdkLogger.Instance.Warn($"current call state is already {currentCall.Status.ToString()}, not process callconnected event");
                return;
            }
            SdkLogger.Instance.Info($"CallID[{currentCall.CallId}], call connected]");

            currentCall.Status = CallStatus.Connected;

            currentCall?.TrigerOnConnected();
        }

        private void OnCallDisconnected(string callId)
        {
            if (callId != currentCall.CallId)
            {
                return;
            }
            if (currentCall.Status >= CallStatus.Disconnected)
            {
                SdkLogger.Instance.Warn($"current call state is already {currentCall.Status.ToString()}, not process calldisconnect event");
                return;
            }
            if(currentCall.CallId == null)
            {
                SdkLogger.Instance.Warn("currentCall.CallId is null");
                return;
            }
            string reason = m_core_telephoneService.getCallEndReason(currentCall.CallId);
            SdkLogger.Instance.Info($"callid[{currentCall.CallId}], call disconnected, release reason is {reason}");
            currentCall.ReleaseReason = ConvertToCallDisconnectReasonType(reason);
            if(!currentCall.IsOne2One)
            {
                OnCallTerminated(callId);
            }
        }


        private void OnCallTerminated(string callId)
        {
            if (callId != currentCall.CallId || !currentCall.IsUsed)
            {
                return;
            }

            if (currentCall.Status ==  CallStatus.Disconnected)
            {
                return;
            }
            currentCall.Status = CallStatus.Disconnected;
            SdkLogger.Instance.Info($"CallID[{callId}], call terminated");          
            currentCall?.TrigerOnDisconnected(currentCall.ReleaseReason);
            currentCall = new Call(this);
        }

        private void OnVideoReady(SCFEventType type, SparkNet.TrackType videoTrackType)
        {
            SdkLogger.Instance.Debug($"{type.ToString()} {videoTrackType.ToString()}");
            switch (type)
            {
                case SCFEventType.RemoteVideoReady:
                    OnRemoteVideoReady(videoTrackType);
                    break;
                case SCFEventType.LocalVideoReady:
                    OnLocalVideoReady();
                    break;
                default:
                    break;
            }
        }
        private void OnRemoteVideoReady(SparkNet.TrackType videoTrackType)
        {
            if (videoTrackType == TrackType.Remote)
            {
                if (currentCall?.MediaOption?.RemoteViewPtr != null
                    && currentCall.MediaOption.RemoteViewPtr.HasValue)
                {
                    currentCall.SetRemoteView(currentCall.MediaOption.RemoteViewPtr.Value);
                }
            }
            else if (videoTrackType >= TrackType.RemoteAux1 && videoTrackType < TrackType.LocalShare)
            {
                var find = currentCall?.AuxStreams.Find(x => (x.Track == 0));

                if (find != null && currentCall.CallId != null)
                {
                    find.Track = videoTrackType;
                    m_core_telephoneService.setView(currentCall.CallId, find.Handle, videoTrackType);
                    currentCall?.TrigerOnAuxStreamEvent(new AuxStreamOpenedEvent(currentCall, find, new WebexApiEventArgs<IntPtr>(true, null, find.Handle)));
                }
            }
        }
        private void OnLocalVideoReady()
        {
            if (currentCall?.MediaOption?.LocalViewPtr != null
                && currentCall.MediaOption.LocalViewPtr.HasValue)
            {
                currentCall.isSendingAudio = true;
                currentCall.SetLocalView(currentCall.MediaOption.LocalViewPtr.Value);
            }
        }
        private void OnRemoteVideoStop(SparkNet.TrackType trackType)
        {    
            if (trackType == TrackType.Remote)
            {
                if (currentCall != null
                    && currentCall.MediaOption != null
                    && currentCall.MediaOption.RemoteViewPtr != null
                    && currentCall.MediaOption.RemoteViewPtr.HasValue)
                {
                    m_core_telephoneService.removeView(currentCall.CallId, currentCall.MediaOption.RemoteViewPtr.Value, trackType);
                }
            }
            else if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.AuxStreams.Find(x =>(x.Track == trackType));
                if (find != null && currentCall.CallId != null)
                {
                    m_core_telephoneService.removeView(currentCall.CallId, find.Handle, trackType);
                    currentCall?.AuxStreams.Remove(find);
                    currentCall?.TrigerOnAuxStreamEvent(new AuxStreamClosedEvent(currentCall, new WebexApiEventArgs<IntPtr>(true, null, find.Handle)));
                }
            }
        }
        private void OnAudioMutedStateChanged(SparkNet.TrackType trackType, string status)
        {
            SdkLogger.Instance.Debug($"{trackType.ToString()} audio is {status}");
            bool isSending = (status != "muted");
            if (trackType == TrackType.Local)
            {
                currentCall?.TrigerOnMediaChanged(new SendingAudioEvent(currentCall, isSending));
            }
            else if (trackType == TrackType.Remote && currentCall.IsRemoteSendingAudio != isSending)
            {
                currentCall.IsRemoteSendingAudio = isSending;
                currentCall?.TrigerOnMediaChanged(new RemoteSendingAudioEvent(currentCall, isSending));
            }
        }

        private void OnMuteRemoteAudioDone(SparkNet.TrackType trackType, string status)
        {
            SdkLogger.Instance.Debug($"local {status} remote audio done. {trackType}");
            currentCall?.TrigerOnMediaChanged(new ReceivingAudioEvent(currentCall, (status != "muted")));
        }
        private void OnMuteRemoteVideoDone(SparkNet.TrackType trackType, string status)
        {
            SdkLogger.Instance.Debug($"local {status} {trackType.ToString()} video done");
            if (trackType == TrackType.Remote)
            {
                currentCall?.TrigerOnMediaChanged(new ReceivingVideoEvent(currentCall, (status != "muted")));
            }
            else if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.AuxStreams.Find(x => (x.Track == trackType));
                if (find != null)
                {
                    find.isReceivingVideo = (status != "muted");
                    currentCall?.TrigerOnAuxStreamEvent(new ReceivingAuxStreamEvent(currentCall, find));
                }
            }
            else if (trackType == TrackType.RemoteShare)
            {
                currentCall?.TrigerOnMediaChanged(new ReceivingShareEvent(currentCall, (status != "muted")));
            }
        }
        private void OnVideoSizeChanged(SparkNet.TrackType trackType)
        {
            SdkLogger.Instance.Debug($"{trackType.ToString()} video size changed");
            if (trackType == TrackType.Local)
            {
                currentCall?.TrigerOnMediaChanged(new LocalVideoViewSizeChangedEvent(currentCall));
            }
            else if (trackType == TrackType.Remote)
            {
                currentCall?.TrigerOnMediaChanged(new RemoteVideoViewSizeChangedEvent(currentCall));
            }
            else if(trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.AuxStreams.Find(x =>(x.Track == trackType));
                if (find != null)
                {
                    SdkLogger.Instance.Debug($"{trackType} person changed");
                    currentCall?.TrigerOnAuxStreamEvent(new AuxStreamSizeChangedEvent(currentCall, find));
                }                
            }
        }
        private void OnDTMFStatusChanged(SparkNet.DTMFCapStatus dtmfStatus)
        {
            currentCall?.TrigerOnCapabiltyChanged(new CapabilitiesDTMF(currentCall, (dtmfStatus == DTMFCapStatus.Enabled)));
        }

        private void OnCameraPreviewReady()
        {
            SdkLogger.Instance.Debug("");
            CameraPreviewReadyEvent?.Invoke(true);
        }

        private void OnRemoteContentSharingStateChanged(SCFEventType type)
        {
            switch (type)
            {
                case SCFEventType.LocalContentSharingStarted:
                    currentCall?.TrigerOnMediaChanged(new SendingShareEvent(currentCall, true));
                    break;
                case SCFEventType.LocalContentSharingStop:
                    currentCall?.TrigerOnMediaChanged(new SendingShareEvent(currentCall, false));
                    break;
                case SCFEventType.RemoteContentSharingStarted:
                    currentCall?.TrigerOnMediaChanged(new RemoteSendingShareEvent(currentCall, true));
                    if (currentCall != null
                        && currentCall.MediaOption.RemoteShareViewPtr.HasValue)
                    {
                        currentCall.SetRemoteShareView(currentCall.MediaOption.RemoteShareViewPtr.Value);
                    }
                    break;
                case SCFEventType.RemoteContentSharingStop:
                    currentCall?.TrigerOnMediaChanged(new RemoteSendingShareEvent(currentCall, false));
                    break;
                case SCFEventType.EnumeratedShareSourcesCallback:
                    currentCall?.TrigerOnSelectShareSource(ShareSourceType.Desktop);
                    break;
                case SCFEventType.EnumeratedAppShareSourcesCallback:
                    currentCall?.TrigerOnSelectShareSource(ShareSourceType.Application);
                    break;
                default:
                    break;
            }
        }
        private CallDisconnectedEvent ConvertToCallDisconnectReasonType(string reason)
        {
            CallDisconnectedEvent result;
            switch (reason)
            {
                case "cancelledByLocalUser":
                    result = new LocalCancel(currentCall);
                    break;
                case "endedByLocalUser":
                    result = ConvertToCallDisconnectReasonTypeEndedByLocalUser();
                    break;
                case "declinedByRemoteUser":
                    result = new RemoteDecline(currentCall);
                    break;
                case "endedByRemoteUser":
                    result = ConvertToCallDisconnectReasonTypeEndedByRemoteUser();
                    break;
                case "endedByLocus":
                    if (currentCall.Direction == Call.CallDirection.Incoming
                        && currentCall.Status < CallStatus.Connected)
                    {
                        result = new RemoteCancel(currentCall);
                    }
                    else
                    {
                        result = new CallError(currentCall, new WebexError(WebexErrorCode.ServiceFailed, reason));
                    }
                    break;
                default:
                    SdkLogger.Instance.Debug($"[{currentCall?.CallId}] reason: {reason}");
                    result = new CallError(currentCall, new WebexError(WebexErrorCode.ServiceFailed, reason));
                    break;
            }
            SdkLogger.Instance.Debug($"convert to {result.GetType().Name}");
            return result;
        }
        private CallDisconnectedEvent ConvertToCallDisconnectReasonTypeEndedByLocalUser()
        {
            CallDisconnectedEvent result;
            if (currentCall.Direction == Call.CallDirection.Incoming
                && currentCall.Status < CallStatus.Connected)
            {
                result = new LocalDecline(currentCall);
            }
            else if (currentCall.Direction == Call.CallDirection.Outgoing
                && currentCall.Status < CallStatus.Connected)
            {
                result = new LocalCancel(currentCall);
            }
            else
            {
                result = new LocalLeft(currentCall);
            }
            return result;
        }
        private CallDisconnectedEvent ConvertToCallDisconnectReasonTypeEndedByRemoteUser()
        {
            CallDisconnectedEvent result;
            if (currentCall.Direction == Call.CallDirection.Incoming
                && currentCall.Status < CallStatus.Connected)
            {
                if (currentCall.IsLocalRejectOrEndCall)
                {
                    result = new LocalDecline(currentCall);
                }
                else
                {
                    result = new OtherDeclined(currentCall);
                }

            }
            else if (currentCall.Direction == Call.CallDirection.Outgoing
                && currentCall.Status < CallStatus.Connected)
            {
                result = new RemoteDecline(currentCall);
            }
            else
            {
                result = new RemoteLeft(currentCall);
            }
            return result;
        }

    }
}
