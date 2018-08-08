
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
        private H264LicensePrompter prompter;

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
            m_core_conversationService = SCFCore.Instance.m_core_conversationService;
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
            m_core_conversationService = null;
            m_core_deviceManager = null;
            isRegisteredToCore = false;
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
        private SparkNet.ConversationService m_core_conversationService;
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
            SDKLogger.Instance.Debug("");

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
            SDKLogger.Instance.Debug("");
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
        /// It supports the following address formats for the receipient:
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
                SDKLogger.Instance.Error($"invalid parameter. address:{address}");
                completedHandler?.Invoke(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid address"), null));
                return;
            }

            if (option == null || completedHandler == null)
            {
                SDKLogger.Instance.Error($"invalid parameter. option or completedHandler is null");
                completedHandler?.Invoke(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.IllegalOperation, "option or completedHandler is null."), null));
                return;
            }

            if (isMercuryConnected == false)
            {
                SDKLogger.Instance.Error("phone is not registered");
                completedHandler(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.Unregistered, "phone is not registered"), null));
                return;
            }

            if (currentCall.IsUsed == true)
            {
                SDKLogger.Instance.Error("Failure: There are other active calls");
                completedHandler(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.IllegalOperation, "There are other active calls"), currentCall));
                return;
            }

            SDKLogger.Instance.Debug($"callee address: {address}, media option: {option.MediaOptionType.ToString()}");

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
                SDKLogger.Instance.Info("video codec license hasn't activated.");

                TriggerOnRequestVideoCodecActivation();
                return;
            }

            ConvertToDialAddress(address, (isSpaceCall, outputAddress) =>
            {
                if (isSpaceCall)
                {
                    if (!m_core_telephoneService.canMakeCall(outputAddress))
                    {
                        //currentCall.init();
                        currentCall = new Call(this);
                        SDKLogger.Instance.Error($"canMakeCall return false. address:{outputAddress}");
                        completedHandler?.Invoke(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.IllegalOperation, "maybe space id is invalid"), null));
                        return;
                    }
                    SDKLogger.Instance.Debug($"This is a space call. join call: {outputAddress}");
                    m_core_telephoneService.joinCall(outputAddress);
                }
                else
                {
                    SDKLogger.Instance.Debug($"This is a direct call. make call: {outputAddress}");
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
            if (option.HasVideo)
            {
                if (!prompter.Check())
                {
                    return false;
                }
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
        /// Prevents Cisco Webex SDK from poping up an Alert for the end user to approve the use of H.264 video codec license from Cisco Systems, Inc.
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
                if (activate == false)
                {
                    SDKLogger.Instance.Warn("reject video codec license");
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
                return;
            }
            //continue call out
            else if (currentCall.Direction == Call.CallDirection.Outgoing && currentCall.Status == CallStatus.Initiated)
            {
                if (activate == false)
                {
                    SDKLogger.Instance.Warn("reject video codec license");
                    currentCall = new Call(this);
                    DialCompletedHandler?.Invoke(new WebexApiEventArgs<Call>(false, new WebexError(WebexErrorCode.RequireH264, ""), null));
                    //currentCall.init();
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
        /// <param name="handle">The preview dispaly window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void StartPreview(IntPtr handle)
        {
            SDKLogger.Instance.Debug($"handle: {handle}");
            StartCaptureDevice(SparkNet.MediaOption.All);
            CameraPreviewReadyEvent = (isReady) =>
            {
                if (isReady == true)
                {
                    m_core_deviceManager.startCameraPreview(handle);
                }
            };          
        }

        /// <summary>
        /// Stop rendering the preview of the local party.
        /// </summary>
        /// <param name="handle">The preview dispaly window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void StopPreview(IntPtr handle)
        {
            SDKLogger.Instance.Debug($"handle: {handle}");
            m_core_deviceManager.removeCameraPreview(handle);
            StopCaptureDevice();
        }

        /// <summary>
        /// Update the preview when video window is resized.
        /// </summary>
        /// <param name="handle">The preview dispaly window handle</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void UpdatePreview(IntPtr handle)
        {
            SDKLogger.Instance.Debug($"handle: {handle}");
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
            SDKLogger.Instance.Debug($"get {type.ToString()} devices");
            SparkNet.Device[] devices = m_core_deviceManager.getDevices((SparkNet.DeviceType)type);
            var list = new List<WebexSDK.AVIODevice>();
            foreach (var d in devices)
            {
                WebexSDK.AVIODevice item = new WebexSDK.AVIODevice();
                item.DefaultDevice = d.defaultDevice;
                item.Id = d.id;
                item.Name = d.name;
                item.Type = (WebexSDK.AVIODeviceType)d.type;
                //item.logString = d.logString;
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
            SDKLogger.Instance.Debug($"select {device.Name}");
            var item = m_core_deviceManager.getDevice(device.Id, (SparkNet.DeviceType)device.Type);
            bool result = m_core_deviceManager.selectDevice(item);
            if (result == true && item.type == SparkNet.DeviceType.Camera)
            {
                currentCall?.TrigerOnMediaChanged(new CameraSwitchedEvent(currentCall, device));
            }
            else if (result == true && item.type == SparkNet.DeviceType.Speaker)
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
            string peopleUrl = "ciscospark://us/PEOPLE/";
            string spaceUrl = "ciscospark://us/ROOM/";

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
            SDKLogger.Instance.Debug($"{option.ToString()}");
            m_core_deviceManager.startCaptureDevice(option);
        }
        private void StopCaptureDevice()
        {
            SDKLogger.Instance.Debug("");
            m_core_deviceManager.stopCaptureDevice();
        }


        private void OnCoreCallBackPhone(SCFEventType type, int error, string status)
        {
            SDKLogger.Instance.Debug("event type:{0}, error[{1}], status:{2}", type.ToString(), error, status);
            switch (type)
            {
                case SCFEventType.ParticipantsChanged:
                    OnParticipantsChanged(error, status);
                    break;
                case SCFEventType.MercuryStateChange:
                    OnMercuryStateChange((SparkNet.MercuryState)error, status); ;
                    break;
                case SCFEventType.JwtAccessTokenExpired:
                    OnJwtAccessTokenExpired();
                    break;
                case SCFEventType.CallStarted:
                    OnCallStarted(error, status);
                    break;
                case SCFEventType.StartRing:
                    OnStartRing((SparkNet.RingerType)error, status);
                    break;
                case SCFEventType.CallCreatedWithConveration:
                    currentCall.CallId = status;
                    break;
                case SCFEventType.CallDisconnected:
                    OnCallDisconnected(error, status); ;
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
                    OnCallTerminated(error, status);
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
                case SCFEventType.VideoMutedStateChanged:
                    //OnVideoMutedStateChanged((SparkNet.TrackType)error, status);
                    break;
                case SCFEventType.MuteRemoteAudioDone:
                    OnMuteRemoteAudioDone((SparkNet.TrackType)error, status);
                    break;
                case SCFEventType.MuteRemoteVideoDone:
                    OnMuteRemoteVideoDone((SparkNet.TrackType)error, status);
                    break;
                case SCFEventType.VideoSizeChanged:
                    OnVideoSizeChanged((SparkNet.TrackType)error, status);
                    break;
                case SCFEventType.DTMFStatus:
                    OnDTMFStatusChanged((SparkNet.DTMFCapStatus)error, status);
                    break;
                case SCFEventType.CameraPreviewReady:
                    OnCameraPreviewReady(error, status);
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
                case SCFEventType.RemoteVideoCountChanged:
                    int newCount = error;
                    if (newCount >= 0)
                    {
                        if (currentCall?.RemoteVideosCount != newCount)
                        {
                            currentCall.RemoteVideosCount = newCount;
                            SDKLogger.Instance.Debug($"remote auxiliary videos count changed to {newCount}");
                            currentCall?.TrigerOnMediaChanged(new RemoteAuxVideosCountChangedEvent(currentCall, newCount));
                        }
                    }
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
                default:
                    break;
            }
        }

        private void OnVideoTrackPersonChanged(TrackType trackType, string callId)
        {
            SDKLogger.Instance.Debug($"trackType[{trackType}], callID[{callId}]");
            if (trackType == TrackType.Remote)
            {
                SDKLogger.Instance.Debug("active speaker changed");
                currentCall?.TrigerOnMediaChanged(new ActiveSpeakerChangedEvent(currentCall, currentCall.ActiveSpeaker));
            }
            else if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.RemoteAuxVideos.Find(x =>(x.track == trackType));
                if (find != null)
                {
                    SDKLogger.Instance.Debug($"{trackType} person changed");
                    currentCall?.TrigerOnMediaChanged(new RemoteAuxVideoPersonChangedEvent(currentCall, find));
                }      
            }
        }

        private void OnIsAuxVideoStreamInUseChanged(TrackType trackType, string callId)
        {
            bool isInUse = m_core_telephoneService.getIsVideoTrackInUse(callId, trackType);
            
            if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.RemoteAuxVideos.Find(x => (x.track == trackType));
                if (find != null)
                {
                    if (find.IsInUse != isInUse)
                    {
                        find.IsInUse = isInUse;
                        SDKLogger.Instance.Debug($"callID[{callId}] trackType[{trackType}] InUse[{isInUse}]");
                    }
                }
            }
        }
        private void OnIsVideoStreamingChanged(TrackType trackType, string callId)
        {
            bool isStreaming = m_core_telephoneService.getIsVideoStreaming(callId, trackType);
            SDKLogger.Instance.Debug($"callID[{callId}] trackType[{trackType}] IsStreaming[{isStreaming}]");

            if (trackType == TrackType.Local)
            {
                if (currentCall?.IsSendingVideo != isStreaming)
                {
                    currentCall.isSendingVideo = isStreaming;
                    currentCall?.TrigerOnMediaChanged(new SendingVideoEvent(currentCall, isStreaming));
                }
            }
            if (trackType == TrackType.Remote)
            {
                if (currentCall?.IsRemoteSendingVideo != isStreaming)
                {
                    currentCall.IsRemoteSendingVideo = isStreaming;
                    currentCall?.TrigerOnMediaChanged(new RemoteSendingVideoEvent(currentCall, isStreaming));
                }
            }
            else if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.RemoteAuxVideos.Find(x => (x.track == trackType));
                if (find != null && find.IsSendingVideo != isStreaming)
                {
                    find.IsSendingVideo = isStreaming;
                    currentCall?.TrigerOnMediaChanged(new RemoteAuxSendingVideoEvent(currentCall, find));
                }
            }
        }
        private void OnIsAudioStreamingChanged(TrackType trackType, string callId)
        {
            SDKLogger.Instance.Debug($"callID[{callId}] trackType[{trackType}]");
            
            if (trackType == TrackType.Remote)
            {
                currentCall.IsRemoteSendingAudio = true;
                currentCall?.TrigerOnMediaChanged(new RemoteSendingAudioEvent(currentCall, true));
            }
        }
        private void OnParticipantsChanged(int error, string callId)
        {
            List<CallMembership> tmpMemberships = new List<CallMembership>();

            if(currentCall == null || currentCall.CallId == null)
            {
                SDKLogger.Instance.Warn("currentCall.CallId is null");
                return;
            }

            if (callId != currentCall.CallId)
            {
                return;
            }

            SparkNet.CallParticipant[] participants = m_core_telephoneService.getCallParticipants(currentCall.CallId);

            if (participants.Count() > 2)
            {
                currentCall.IsGroup = true;
                SDKLogger.Instance.Debug($"CallID[{currentCall.CallId}], this is a meeting call");
            }
            else
            {
                currentCall.IsGroup = false ;
                SDKLogger.Instance.Debug($"CallID[{currentCall.CallId}], this is a one2one call");
            }

            foreach (var item in participants)
            {
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
                    if (newItem.State == CallMembership.CallState.Joined)
                    {
                        if(false == ProcessParticipantJoined(newItem))
                        {
                            return;
                        }
                        currentCall?.TrigerOnCallMembershipChanged(new CallMembershipJoinedEvent(currentCall,newItem));
                    }
                    else if (newItem.State == CallMembership.CallState.Declined)
                    {
                        currentCall?.TrigerOnCallMembershipChanged(new CallMembershipDeclinedEvent(currentCall, newItem));

                    }
                    else if (newItem.State == CallMembership.CallState.Left)
                    {
                        currentCall?.TrigerOnCallMembershipChanged(new CallMembershipLeftEvent(currentCall, newItem));

                    }

                    if (findMembership == null)
                    {
                        continue;
                    }
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

        private bool ProcessParticipantJoined(CallMembership newItem)
        {
            //one2one outgoing call, triger callConnected event when remote participant joined
            if (currentCall?.IsGroup == false && currentCall?.Direction == Call.CallDirection.Outgoing)
            {
                if (newItem.IsSelf != true)
                {
                    currentCall.IsSignallingConnected = true;
                    OnCallConnected();
                }
            }
            else
            {
                if (newItem.IsSelf == true)
                {
                    bool find = false;
                    foreach (var device in newItem.Devices)
                    {
                        //self and this device join, triger call connected
                        if (m_core.getDeviceUrl() == device.url
                            && ConvertToCallMembershipStateEnum(device.state) == CallMembership.CallState.Joined)
                        {
                            find = true;
                            currentCall.IsSignallingConnected = true;
                            OnCallConnected();
                            break;
                        }
                    }
                    //self and other device join, triger call release
                    if (!find)
                    {
                        currentCall.ReleaseReason = new OtherConnected(currentCall);
                        currentCall?.TrigerOnDisconnected(currentCall.ReleaseReason);
                        //currentCall?.init();
                        currentCall = new Call(this);
                        return false;
                    }
                }
            }
            return true;
        }
        private void OnMercuryStateChange(SparkNet.MercuryState state, string status)
        {
            SDKLogger.Instance.Info($"state: {state.ToString()}");
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
                        SDKLogger.Instance.Info("when mercury disconnected, access token fail.");
                    }
                });
            }

        }
        private void OnJwtAccessTokenExpired()
        {
            SDKLogger.Instance.Info("");
            authenticator?.RefreshToken(r =>
            {
                if (!r.IsSuccess)
                {
                    SDKLogger.Instance.Info("access token failed.");
                }
            });
        }
        private void OnCallStarted(int error, string callId)
        {
            SDKLogger.Instance.Debug($"CallId[{callId}] error[{error}]");
            if (currentCall.CallId != null)
            {
                SDKLogger.Instance.Warn("already have a call");
                return;
            }

            // outgoing call
            if (currentCall.IsUsed == true && currentCall.Direction == Call.CallDirection.Outgoing)
            {
                currentCall.CallId = callId;
                m_core_telephoneService.setMediaOption(currentCall.CallId , currentCall.MediaOption.MediaOptionType);
                m_core_telephoneService.setAudioMaxBandwidth(currentCall.CallId, AudioMaxBandwidth);
                m_core_telephoneService.setVideoMaxBandwidth(currentCall.CallId, VideoMaxBandwidth);
                DialCompletedHandler?.Invoke(new WebexApiEventArgs<Call>(true, null, currentCall));
            }
        }

        private void OnShowIncomingCallAlert(int error, string callId)
        {
            // incoming call
            SDKLogger.Instance.Debug($"CallId[{callId}] error[{error}]");
            if (currentCall.CallId != null)
            {
                SDKLogger.Instance.Warn("already have a call, reject incoming call");
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
                SDKLogger.Instance.Warn($"current call state is already {currentCall.Status}, not process startring event");
                return;
            }
            SDKLogger.Instance.Info($"CallID[{callId}], ringer type is {ringerType.ToString()}");

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
                SDKLogger.Instance.Warn($"current call state is already {currentCall.Status.ToString()}, not process callconnected event");
                return;
            }
            SDKLogger.Instance.Info($"CallID[{currentCall.CallId}], call connected]");

            currentCall.Status = CallStatus.Connected;

            currentCall?.TrigerOnConnected();
        }

        private void OnCallDisconnected(int error, string callId)
        {
            if (callId != currentCall.CallId)
            {
                return;
            }
            if (currentCall.Status >= CallStatus.Disconnected)
            {
                SDKLogger.Instance.Warn($"current call state is already {currentCall.Status.ToString()}, not process calldisconnect event");
                return;
            }
            if(currentCall.CallId == null)
            {
                SDKLogger.Instance.Warn("currentCall.CallId is null");
                return;
            }
            string reason = m_core_telephoneService.getCallEndReason(currentCall.CallId);
            SDKLogger.Instance.Info($"callid[{currentCall.CallId}], call disconnected, release reason is {reason}");
            currentCall.ReleaseReason = ConvertToCallDisconnectReasonType(reason);
        }


        private void OnCallTerminated(int error, string callId)
        {
            if (callId != currentCall.CallId)
            {
                return;
            }

            if (currentCall.Status ==  CallStatus.Disconnected)
            {
                return;
            }
            currentCall.Status = CallStatus.Disconnected;
            SDKLogger.Instance.Info($"CallID[{callId}], call terminated");          
            currentCall?.TrigerOnDisconnected(currentCall.ReleaseReason);
            currentCall = new Call(this);
        }

        private void OnVideoReady(SCFEventType type, SparkNet.TrackType videoTrackType)
        {
            SDKLogger.Instance.Debug($"{type.ToString()} {videoTrackType.ToString()}");
            switch (type)
            {
                case SCFEventType.RemoteVideoReady:       
                    if (videoTrackType == TrackType.Remote)
                    {
                        if (currentCall != null
                            && currentCall.MediaOption != null
                            && currentCall.MediaOption.RemoteViewPtr != null
                            && currentCall.MediaOption.RemoteViewPtr.HasValue)
                        {
                            currentCall.SetRemoteView(currentCall.MediaOption.RemoteViewPtr.Value);
                        }
                    }
                    else if (videoTrackType >= TrackType.RemoteAux1 && videoTrackType < TrackType.LocalShare)
                    {
                        int index = (int)videoTrackType - (int)TrackType.RemoteAux1;
                        var find = currentCall?.RemoteAuxVideos.Find(x =>(x.track == 0));

                        if (find != null && currentCall.CallId != null)
                        {
                            find.track = videoTrackType;
                            foreach (var item in find.HandleList)
                            {
                                m_core_telephoneService.setView(currentCall.CallId, item, videoTrackType);
                            }
                        }
                    }
                    break;
                case SCFEventType.LocalVideoReady:
                    if (currentCall != null
                        && currentCall.MediaOption != null
                        && currentCall.MediaOption.LocalViewPtr != null
                        && currentCall.MediaOption.LocalViewPtr.HasValue)
                    {
                        currentCall.isSendingAudio = true;
                        currentCall.SetLocalView(currentCall.MediaOption.LocalViewPtr.Value);
                    }
                    break;
                default:
                    break;
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
                var find = currentCall?.RemoteAuxVideos.Find(x =>(x.track == trackType));
                if (find != null && currentCall.CallId != null)
                {
                    var handleList = new List<IntPtr>(find.HandleList);
                    foreach (var item in handleList)
                    {
                        find.RemoveViewHandle(item);
                    }
                    currentCall?.RemoteAuxVideos.Remove(find);
                }
            }
        }
        private void OnAudioMutedStateChanged(SparkNet.TrackType trackType, string status)
        {
            SDKLogger.Instance.Debug($"{trackType.ToString()} audio is {status}");
            bool isSending = !(status == "muted");
            if (trackType == TrackType.Local)
            {
                currentCall?.TrigerOnMediaChanged(new SendingAudioEvent(currentCall, isSending));
            }
            else if (trackType == TrackType.Remote)
            {
                if (currentCall.IsRemoteSendingAudio != isSending)
                {
                    currentCall.IsRemoteSendingAudio = isSending;
                    currentCall?.TrigerOnMediaChanged(new RemoteSendingAudioEvent(currentCall, isSending));
                }
            }
        }

        private void OnVideoMutedStateChanged(SparkNet.TrackType trackType, string status)
        {
            SDKLogger.Instance.Debug($"{trackType.ToString()} video is {status}");
            bool isSending = !(status == "muted");

            if (trackType == TrackType.Local)
            {
                if (currentCall?.IsSendingVideo != isSending)
                {
                    currentCall.isSendingVideo = isSending;
                    currentCall?.TrigerOnMediaChanged(new SendingVideoEvent(currentCall, isSending));
                }
            }
            else if (trackType == TrackType.Remote)
            {
                if (currentCall?.IsRemoteSendingVideo != isSending)
                {
                    currentCall.IsRemoteSendingVideo = isSending;
                    currentCall?.TrigerOnMediaChanged(new RemoteSendingVideoEvent(currentCall, isSending));
                }
            }
            else if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.RemoteAuxVideos.Find(x => (x.track == trackType));
                if (find != null)
                {
                    if (find.IsSendingVideo != isSending)
                    {
                        find.IsSendingVideo = isSending;
                        currentCall?.TrigerOnMediaChanged(new RemoteAuxSendingVideoEvent(currentCall, find));
                    }
                }
            }
        }

        private void OnMuteRemoteAudioDone(SparkNet.TrackType trackType, string status)
        {
            SDKLogger.Instance.Debug($"local {status} remote audio done");
            currentCall?.TrigerOnMediaChanged(new ReceivingAudioEvent(currentCall, !(status == "muted")));
        }
        private void OnMuteRemoteVideoDone(SparkNet.TrackType trackType, string status)
        {
            SDKLogger.Instance.Debug($"local {status} {trackType.ToString()} video done");
            if (trackType == TrackType.Remote)
            {
                currentCall?.TrigerOnMediaChanged(new ReceivingVideoEvent(currentCall, !(status == "muted")));
            }
            else if (trackType >= TrackType.RemoteAux1 && trackType < TrackType.LocalShare)
            {
                var find = currentCall?.RemoteAuxVideos.Find(x => (x.track == trackType));
                if (find != null)
                {
                    find.isReceivingVideo = !(status == "muted");
                    currentCall?.TrigerOnMediaChanged(new ReceivingAuxVideoEvent(currentCall, find));
                }
            }
            else if (trackType == TrackType.RemoteShare)
            {
                currentCall?.TrigerOnMediaChanged(new ReceivingShareEvent(currentCall, !(status == "muted")));
            }
        }
        private void OnVideoSizeChanged(SparkNet.TrackType trackType, string status)
        {
            SDKLogger.Instance.Debug($"{trackType.ToString()} video size changed");
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
                var find = currentCall?.RemoteAuxVideos.Find(x =>(x.track == trackType));
                if (find != null)
                {
                    SDKLogger.Instance.Debug($"{trackType} person changed");
                    currentCall?.TrigerOnMediaChanged(new RemoteAuxVideoSizeChangedEvent(currentCall, find));
                }                
            }
        }
        private void OnDTMFStatusChanged(SparkNet.DTMFCapStatus dtmfStatus, string status)
        {
            currentCall?.TrigerOnCapabiltyChanged(new CapabilitieDTMF(currentCall, (dtmfStatus == DTMFCapStatus.Enabled)));
        }

        private void OnCameraPreviewReady(int error, string status)
        {
            SDKLogger.Instance.Debug("");
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
                    break;
                case "declinedByRemoteUser":
                    result = new RemoteDecline(currentCall);
                    break;
                case "endedByRemoteUser":
                    if (currentCall.Direction == Call.CallDirection.Incoming
                        && currentCall.Status < CallStatus.Connected)
                    {
                        if (currentCall.IsLocalRejectOrEndCall == true)
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
                case "dialTimeoutReached":
                case "cancelledByLocalError":             
                case "endedByReconnectTimeout":
                case "wirelessShareTimeoutReached":
                case "networkUnavailable":
                default:
                    result = new CallError(currentCall, new WebexError(WebexErrorCode.ServiceFailed, reason));
                    break;
            }
            SDKLogger.Instance.Debug($"convert to {result.GetType().Name}");
            return result;
        }

    }
}
