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


using SparkNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WebexSDK
{

    /// <summary>
    /// Messages are how we communicate in a space. In Webex, each message is displayed on its own line along with a timestamp and sender information. 
    /// Use this API to list, create, and delete messages. Message can contain plain text, rich text, and a file attachment.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class MessageClient
    {

        #region private properties

        private MessageClient()
        {    
        }
        private MessageClient(IAuthenticator authenticator) : this()
        {
            this.authenticator = authenticator;
            RegisterToCore();
        }
        private readonly Dictionary<string, SessionAction> sessionActions = new Dictionary<string, SessionAction>();
        private readonly Dictionary<string, MessageAction> messageActions = new Dictionary<string, MessageAction>();
        private readonly Dictionary<string, FileAction> fileActions = new Dictionary<string, FileAction>();

        readonly IAuthenticator authenticator;
        private static volatile MessageClient instance = null;
        private static readonly object lockHelper = new object();
        private bool isRegisteredToCore = false;

        private SparkNet.CoreFramework m_core;
        private SparkNet.ConversationService m_core_conversationService;
        private SparkNet.ImageService m_core_imageService;
        #endregion

        /// <summary>
        /// Message callback event, such as a message arrived or is deleted.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public Action<MessageEvent> OnEvent { get; set; }

        /// <summary>
        /// Lists all messages in a space by space Id.
        /// If present, it includes the associated media content attachment for each message.
        /// The list sorts the messages in descending order by creation date.
        /// </summary>
        /// <param name="spaceId">The identifier of the space.</param>
        /// <param name="mentionedPeople">Only list messages mentioned self</param>
        /// <param name="before">Only list messages sent only before this date</param>
        /// <param name="max">The maximum number of messages in the response, default is 50</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void List(string spaceId, string mentionedPeople, DateTime before, int? max, Action<WebexApiEventArgs<List<Message>>> completionHandler)
        {
            List(spaceId, mentionedPeople, before, null, max, completionHandler);
        }

        /// <summary>
        /// Lists all messages in a space by space Id.
        /// If present, it includes the associated media content attachment for each message.
        /// The list sorts the messages in descending order by creation date.
        /// </summary>
        /// <param name="spaceId">The identifier of the space.</param>
        /// <param name="mentionedPeople">Only list messages mentioned self</param>
        /// <param name="beforeMessage">only list messages sent only before this message by id.</param>
        /// <param name="max">The maximum number of messages in the response, default is 50.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void List(string spaceId, string mentionedPeople, string beforeMessage, int? max, Action<WebexApiEventArgs<List<Message>>> completionHandler)
        {
            List(spaceId, mentionedPeople, null, beforeMessage, max, completionHandler);
        }

        /// <summary>
        /// Posts a plain text message, and optionally, mentions group and a media content attachment, to a space by space Id.
        /// </summary>
        /// <param name="spaceId">The identifier of the space where the message is to be posted.</param>
        /// <param name="text">The plain text message to be posted to the space.</param>
        /// <param name="mentions">The mention items to be posted to the space.</param>
        /// <param name="files">Local file objects to be uploaded to the space.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void PostToSpace(string spaceId, string text, List<Mention> mentions = null, List<LocalFile> files = null, Action<WebexApiEventArgs<Message>> completionHandler = null)
        {
            Post(spaceId, null, text, mentions, files, completionHandler);
        }

        /// <summary>
        /// Posts a private 1:1 message in plain text, and optionally, a media content attachment, to a person by person email.
        /// </summary>
        /// <param name="toPerson">The email address or the personId of the recipient when sending a private 1:1 message.</param>
        /// <param name="text">The plain text message to post to the space.</param>
        /// <param name="files">Local file objects to be uploaded to the space.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void PostToPerson(string toPerson, string text, List<LocalFile> files = null, Action<WebexApiEventArgs<Message>> completionHandler = null)
        {
            Post(null, toPerson, text, null, files, completionHandler);
        }

        /// <summary>
        /// Retrieves the details for a message by space Id and message Id.
        /// </summary>
        /// <param name="spaceId">The identifier of the space.</param>
        /// <param name="messageId">The identifier of the message.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Get(string spaceId, string messageId, Action<WebexApiEventArgs<Message>> completionHandler)
        {
            string conversationId = null;
            string parsedMessageId = null;

            if (spaceId == null
                || StringExtention.ParseHydraId(spaceId, ref conversationId) != StringExtention.HydraIdType.Space)
            {
                completionHandler?.Invoke(new WebexApiEventArgs<Message>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid spaceId parameter"), null));
                return;
            }
            
            if (messageId == null
                || StringExtention.ParseHydraId(messageId, ref parsedMessageId) != StringExtention.HydraIdType.Message)
            {
                completionHandler?.Invoke(new WebexApiEventArgs<Message>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid beforeMessage parameter"), null));
                return;
            }

            var coversation = m_core_conversationService.getConversation(conversationId);
            if (coversation == null)
            {
                completionHandler?.Invoke(new WebexApiEventArgs<Message>(false, null, null));
                return;
            }
            var message = coversation.getMessage(parsedMessageId);
            if (message == null)
            {
                completionHandler?.Invoke(new WebexApiEventArgs<Message>(false, null, null));
                return;
            }

            completionHandler?.Invoke(new WebexApiEventArgs<Message>(true, null, ToMessage(conversationId, message)));
        }

        /// <summary>
        /// Retrieves the details for a message by message Id.
        /// </summary>
        /// <param name="messageId">The identifier of the message.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Get(string messageId, Action<WebexApiEventArgs<Message>> completionHandler)
        {
            string parsedMessageId = null;
            if (messageId == null
                || StringExtention.ParseHydraId(messageId, ref parsedMessageId) != StringExtention.HydraIdType.Message)
            {
                completionHandler?.Invoke(new WebexApiEventArgs<Message>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid beforeMessage parameter"), null));
                return;
            }

            string[] conversations = m_core_conversationService.getConversations();
            foreach (var conversationId in conversations)
            {
                var coversation = m_core_conversationService.getConversation(conversationId);
                if (coversation == null)
                {
                    continue;
                }
                var message = coversation.getMessage(parsedMessageId);
                if (message == null || message.getPublishTime() == 0)
                {
                    continue;
                }

                completionHandler?.Invoke(new WebexApiEventArgs<Message>(true, null, ToMessage(conversationId, message)));
                return;
            }

            completionHandler?.Invoke(new WebexApiEventArgs<Message>(false, null, null));
        }

        /// <summary>
        /// Deletes a message by message id.
        /// </summary>
        /// <param name="messageId">The identifier of the message.</param>
        /// <param name="completionHandler">The completion event handler.</param>
        /// <remarks>Since: 0.1.0</remarks>
        public void Delete(string messageId, Action<WebexApiEventArgs> completionHandler)
        {
            string parsedMessageId = null;
            if (messageId == null
                || StringExtention.ParseHydraId(messageId, ref parsedMessageId) != StringExtention.HydraIdType.Message)
            {
                completionHandler?.Invoke(new WebexApiEventArgs(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid beforeMessage parameter")));
                return;
            }

            if (messageActions.ContainsKey(parsedMessageId))
            {
                messageActions[parsedMessageId].deleteCompletionHandler = completionHandler;
            }
            else
            {
                messageActions.Add(parsedMessageId, new MessageAction()
                {
                    deleteCompletionHandler = completionHandler
                });
            }
            m_core_conversationService.deleteMessage(parsedMessageId);
        }

        /// <summary>
        /// Download a file object, save the file to pointed destination.
        /// </summary>
        /// <param name="file">The RemoteFile object need to be downloaded.</param>
        /// <param name="to">The local file directory for saving downloaded file.</param>
        /// <param name="progressHandler">The download progress indicator.</param>
        public void DownloadFile(RemoteFile file, string to, Action<WebexApiEventArgs<int>> progressHandler)
        {
            string parsedConversationId = null;
            string parsedMessageId = null;
            if (file == null
                || StringExtention.ParseHydraId(file.SpaceId, ref parsedConversationId) != StringExtention.HydraIdType.Space
                || StringExtention.ParseHydraId(file.MessageId, ref parsedMessageId) != StringExtention.HydraIdType.Message)
            {
                progressHandler?.Invoke(new WebexApiEventArgs<int>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid file parameter."), 0));
                return;
            }

            if (to == null)
            {
                progressHandler?.Invoke(new WebexApiEventArgs<int>(false, new WebexError(WebexErrorCode.IllegalOperation, "illegal download path"), 0));
                return;
            }

            string fileId = GetFileId(parsedConversationId, parsedMessageId, file.FileIndex);
            if (fileActions.ContainsKey(fileId))
            {
                fileActions[fileId].downloadProgressHandler = progressHandler;
            }
            else
            {
                fileActions.Add(fileId, new FileAction()
                {
                    downloadProgressHandler = progressHandler
                });
            }

            m_core_conversationService?.downloadSharedContent(parsedConversationId, parsedMessageId, file.FileIndex, to);
        }

        /// <summary>
        /// Download a file thumbnail, save the thumbnail to pointed destination.
        /// </summary>
        /// <param name="file">The RemoteFile object</param>
        /// <param name="to">The local file directory for saving downloaded file.</param>
        /// <param name="completionHandler">The full path of the downloaded file.</param>
        public void DownloadThumbnail(RemoteFile file, string to, Action<WebexApiEventArgs<string>> completionHandler)
        {
            string parsedConversationId = null;
            string parsedMessageId = null;

            if (file == null
                || StringExtention.ParseHydraId(file.SpaceId, ref parsedConversationId) != StringExtention.HydraIdType.Space
                || StringExtention.ParseHydraId(file.MessageId, ref parsedMessageId) != StringExtention.HydraIdType.Message)
            {
                completionHandler?.Invoke(new WebexApiEventArgs<string>(false, new WebexError(WebexErrorCode.IllegalOperation, "parameter invalid."), null));
                return;
            }

            string path = to;
            //default download path
            if (to == null)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + "\\thumbnails\\";
                try
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                catch
                {
                    SdkLogger.Instance.Error($"create download path[{path}] failed.");
                }
            }
            string name = "thumb-" + Guid.NewGuid().ToString() + "-" + file.Name;
            var fullPath = path + name;

            SparkNet.Image image = m_core_imageService?.getContentThumbnailImage(parsedConversationId, parsedMessageId, file.FileIndex);
            if (image != null)
            {
                if (SaveImageToLocal(image.imageBuffer, fullPath))
                {
                    completionHandler?.Invoke(new WebexApiEventArgs<string>(true, null, fullPath));
                }
                else
                {
                    completionHandler?.Invoke(new WebexApiEventArgs<string>(false, new WebexError(WebexErrorCode.IllegalOperation, "save failed."), null));
                }                
            }
            else
            {
                string fileId = GetFileId(parsedConversationId, parsedMessageId, file.FileIndex);
                if (fileActions.ContainsKey(fileId))
                {
                    fileActions[fileId].downloadThumbnailAction = new FileAction.DownloadThumbnailAction()
                    {
                        path = fullPath,
                        completionHandler = completionHandler
                    };
                }
                else
                {
                    fileActions.Add(fileId, new FileAction()
                    {
                        downloadThumbnailAction = new FileAction.DownloadThumbnailAction()
                        {
                            path = fullPath,
                            completionHandler = completionHandler
                        }
                    });
                }

            }
        }

        private void RegisterToCore()
        {
            if (isRegisteredToCore)
            {
                return;
            }
            m_core = SCFCore.Instance.m_core;
            m_core_conversationService = SCFCore.Instance.m_core_conversationService;
            m_core_imageService = SCFCore.Instance.m_core_imageService;
            m_core.m_CallbackEvent += OnCoreCallBackMessage;
            isRegisteredToCore = true;
        }
        internal void UnRegisterToCore()
        {
            if (!isRegisteredToCore)
            {
                return;
            }
            isRegisteredToCore = false;
            m_core.m_CallbackEvent -= OnCoreCallBackMessage;
            m_core = null;
            m_core_conversationService = null;
            instance = null;
        }
        internal static MessageClient GetInstance(IAuthenticator authenticator)
        {
            if (null == instance)
            {
                lock (lockHelper)
                {
                    if (null == instance)
                    {
                        instance = new MessageClient(authenticator);
                    }
                }
            }
            return instance;
        }

        private void List(string spaceId, string mentionedPeople, DateTime? before, string beforeMessage, int? max, Action<WebexApiEventArgs<List<Message>>> completionHandler)
        {
            string conversationId = null;
            string mentionedPeopleId = null;
            string beforeMessageId = null;

            if (StringExtention.ParseHydraId(spaceId, ref conversationId) != StringExtention.HydraIdType.Space)
            {
                SdkLogger.Instance.Error("spaceId format is invalid.");
                completionHandler?.Invoke(new WebexApiEventArgs<List<Message>>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid spaceId parameter"), null));
                return;
            }
            if (mentionedPeople != null
                && mentionedPeople != "me"
                && StringExtention.ParseHydraId(mentionedPeople, ref mentionedPeopleId) != StringExtention.HydraIdType.People)
            {
                SdkLogger.Instance.Error("mentionedPeople format is invalid.");
                completionHandler?.Invoke(new WebexApiEventArgs<List<Message>>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid mentionedPeople parameter"), null));
                return;
            }
            if (beforeMessage != null
                && StringExtention.ParseHydraId(beforeMessage, ref beforeMessageId) != StringExtention.HydraIdType.Message)
            {
                SdkLogger.Instance.Error("before message id is invalid.");
                completionHandler?.Invoke(new WebexApiEventArgs<List<Message>>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid beforeMessage parameter"), null));
                return;
            }
            // default limit max list count
            int? maxCount = (max == null ? 50 : max);

            SdkLogger.Instance.Info($"spaceId[{conversationId}] mentionedPeople[{mentionedPeople}] beforeTime[{before}] beforeMessage[{beforeMessage}] Max[{maxCount}]");

            var coversation = m_core_conversationService.getConversation(conversationId);
            var messages = coversation.getMessages();
            string firstMessageId = messages[0];

            if (IsFetchEnough(conversationId, mentionedPeopleId, before, beforeMessageId, maxCount, out List<Message> listMsg))
            {
                SdkLogger.Instance.Info($"Successe list {listMsg.Count} messages");
                completionHandler?.Invoke(new WebexApiEventArgs<List<Message>>(true, null, listMsg));
            }
            else
            {
                if (sessionActions.ContainsKey(conversationId) && sessionActions[conversationId].list != null)
                {
                    SdkLogger.Instance.Error("last list operation hasn't finished");
                    completionHandler?.Invoke(new WebexApiEventArgs<List<Message>>(false, new WebexError(WebexErrorCode.IllegalOperation, "last list operation hasn't finished"), null));
                    return;
                }
                else
                {
                    sessionActions.Add(conversationId, new SessionAction()
                    {
                        list = new SessionAction.ListAction()
                        {
                            listMessageCount = maxCount,
                            listCompletionHandler = completionHandler,
                            conversationId = conversationId,
                            beforeMessageId = beforeMessageId,
                            before = before,
                            mentionedPeople = mentionedPeople
                        }
                    });
                }

                SdkLogger.Instance.Debug("Fetch more message. first message [firstMessageId]");
                m_core_conversationService.fetchMoreMessage(conversationId, firstMessageId, true);
            }

        }

        private void MessageFilter(SCFEventType type, string conversationId, string messageId)
        {
            var conversation = m_core_conversationService.getConversation(conversationId);
            SparkNet.Message arrivedMsg = conversation.getMessage(messageId);
            var msgType = arrivedMsg.getMessageType();
            
            // don't process provisional message, for example when sending a message, 
            // you will got a provisional message first and then you will get a message to change it's message id with a new id.
            if (arrivedMsg.isProvisional())
            {
                return;
            }
            SdkLogger.Instance.Debug($"arrived message type is {msgType.ToString()} [{messageId}]");

            if (type == SCFEventType.MessageArrived)
            {
                if (msgType == SparkNet.MessageType.NormalMessage)
                {
                    SdkLogger.Instance.Info($"receive a message [{arrivedMsg.getGuid()}]");
                    OnEvent?.Invoke(new MessageArrived(ToMessage(conversationId, arrivedMsg)));
                }
            }
            else if (type == SCFEventType.MessageChanged && msgType == SparkNet.MessageType.TombStone)
            {
                if (messageActions.ContainsKey(messageId))
                {
                    messageActions[messageId].deleteCompletionHandler?.Invoke(new WebexApiEventArgs(true, null));
                    messageActions[messageId].deleteCompletionHandler = null;
                    if (IsMessageActionIsNull(messageActions[messageId]))
                    {
                        messageActions.Remove(messageId);
                    }
                }
                string msgId = StringExtention.EncodeHydraId(StringExtention.HydraIdType.Message, messageId);
                OnEvent?.Invoke(new MessageDeleted(msgId));
            }
            // ignore other type of messages

        }

        private void OnCoreCallBackMessage(SCFEventType type, int error, string status)
        {
            SdkLogger.Instance.Debug("event type:{0}, error[{1}], status:{2}", type.ToString(), error, status);
            string[] arrStr = status.Trim().Split(' ');
            switch (type)
            {
                case SCFEventType.MessageArrived:
                case SCFEventType.MessageChanged:
                    // strStatus= "conversationid"+"message ids"
                    OnMessageArrivedAndChanged(type, status);
                    break;
                case SCFEventType.ConversationIdChanged:
                    // strStatus = "old conversationId"+"new conversationId"
                    OnConversationIdChanged(arrStr[0], arrStr[1]);
                    break;
                case SCFEventType.MessageIdChanged:
                    // strStatus= "conversationid"+"new message id"+"old message id"
                    OnMessageIdChanged(arrStr[0], arrStr[1], arrStr[2]);
                    break;
                case SCFEventType.DownloadProgress:
                    // strStatus = "conversationId"+"messageId"+"contentIndex"+"progress"
                    OnDownloadProgress(arrStr[0], arrStr[1], arrStr[2], arrStr[3]);
                    break;
                case SCFEventType.UploadProgress:
                    // strStatus = "conversationId"+"messageId"+"contentIndex"+"progress"
                    OnUploadProgress(arrStr[0], arrStr[1], arrStr[2], arrStr[3]);
                    break;
                case SCFEventType.DownloadComplete:
                    // strStatus = "conversationId" + "messageId" + "contentIndex" + "downloadFileName"
                    OnDownloadComplete(arrStr[0], arrStr[1], arrStr[2], arrStr[3]);
                    break;
                case SCFEventType.DownloadFailed:
                    // strStatus = "conversationId"+"messageId"+"contentIndex"+"errorCode"
                    OnDownloadFailed(arrStr[0], arrStr[1], arrStr[2], arrStr[3]);
                    break;
                case SCFEventType.ThumbnailChanged:
                    // strStatus = "conversationId"+"messageId"+"contentIndex"
                    OnThumbnailChanged(arrStr[0], arrStr[1], arrStr[2]);
                    break;
                case SCFEventType.ThumbnailDownloadFailed:
                    // strStatus = "conversationId"+"messageId"+"contentIndex"+"errorCode"
                    OnThumbnailDownloadFailed(arrStr[0], arrStr[1], arrStr[2], arrStr[3]);
                    break;

                default:
                    break;
            }
        }
        private void OnMessageArrivedAndChanged(SCFEventType type, string status)
        {
            // strStatus= "conversationid"+"message ids"
            string[] arrStr = status.Trim().Split(' ');
            string conversationId = arrStr[0];
            string messageId = arrStr[1];

            // new message arrived
            if (arrStr.Length == 2)
            {
                MessageFilter(type, conversationId, messageId);
                return;
            }
            // fetch more message result
            else if (sessionActions.ContainsKey(conversationId))
            {
                var listAction = sessionActions[conversationId].list;
                if (listAction != null)
                {
                    if (IsFetchEnough(listAction.conversationId, listAction.mentionedPeople, listAction.before, listAction.beforeMessageId, listAction.listMessageCount, out List<Message> listMsg))
                    {
                        SdkLogger.Instance.Info($"List {listMsg.Count} messages.");
                        listAction.listCompletionHandler?.Invoke(new WebexApiEventArgs<List<Message>>(true, null, listMsg));
                        sessionActions[conversationId].list = null;
                        if (IsSessionActionIsNull(sessionActions[conversationId]))
                        {
                            sessionActions.Remove(conversationId);
                        }
                    }
                    else
                    {
                        m_core_conversationService.fetchMoreMessage(listAction.conversationId, messageId, true);
                    }
                }
            }
        }
        private void OnConversationIdChanged(string oldId, string newId)
        {
            if (creatOne2OneSpaceCompletionHandler.ContainsKey(oldId))
            {
                SdkLogger.Instance.Debug($"conversationId changed from old [{oldId}] to new [{newId}]");
                creatOne2OneSpaceCompletionHandler[oldId].Invoke(newId);
                creatOne2OneSpaceCompletionHandler.Remove(oldId);
            }
        }
        private void OnMessageIdChanged(string conversationid, string newId, string oldId)
        {
            var conversation = m_core_conversationService.getConversation(conversationid);
            SparkNet.Message arrivedMsg = conversation.getMessage(newId);
            if (messageActions.ContainsKey(oldId))
            {
                var message = ToMessage(conversationid, arrivedMsg);
                SdkLogger.Instance.Debug($"message id changed from old [{oldId}] to new [{newId}]");
                messageActions[oldId].postCompletionHandler?.Invoke(new WebexApiEventArgs<Message>(true, null, message));
                messageActions[oldId].postCompletionHandler = null;
                if (IsMessageActionIsNull(messageActions[oldId]))
                {
                    messageActions.Remove(oldId);
                }
            }
        }
        private void OnDownloadProgress(string conversationId, string messageId, string contentIndex, string strProgress)
        {
            string fileId = GetFileId(conversationId, messageId, contentIndex);
            if (fileActions.ContainsKey(fileId))
            {
                var progress = Convert.ToInt32(strProgress);
                SdkLogger.Instance.Info($"{fileId} download progress is {progress}");
                fileActions[fileId].downloadProgressHandler?.Invoke(new WebexApiEventArgs<int>(true, null, progress));
            }
        }
        private void OnUploadProgress(string conversationId, string messageId, string contentIndex, string strProgress)
        {
            string fileId = GetFileId(conversationId, messageId, contentIndex);
            if (fileActions.ContainsKey(fileId))
            {
                var progress = Convert.ToInt32(strProgress);
                SdkLogger.Instance.Info($"{fileId} upload progress is {progress}");
                fileActions[fileId].uploadProgressHandler?.Invoke(new WebexApiEventArgs<int>(true, null, progress));
                if (progress == 100)
                {
                    fileActions[fileId].uploadProgressHandler = null;
                    if (IsFileActionIsNull(fileActions[fileId]))
                    {
                        fileActions.Remove(fileId);
                    }
                }
            }
        }
        private void OnDownloadComplete(string conversationId, string messageId, string contentIndex, string downloadFileName)
        {
            string fileId = GetFileId(conversationId, messageId, contentIndex);
            if (fileActions.ContainsKey(fileId))
            {
                SdkLogger.Instance.Info($"{fileId} {downloadFileName} download complete");
                fileActions[fileId].downloadProgressHandler = null;
                if (IsFileActionIsNull(fileActions[fileId]))
                {
                    fileActions.Remove(fileId);
                }
            }
        }
        private void OnDownloadFailed(string conversationId, string messageId, string contentIndex, string errorCode)
        {
            string fileId = GetFileId(conversationId, messageId, contentIndex);
            if (fileActions.ContainsKey(fileId))
            {
                SdkLogger.Instance.Error($"{fileId} download faild for {errorCode}.");
                fileActions[fileId].downloadProgressHandler?.Invoke(new WebexApiEventArgs<int>(false, new WebexError(WebexErrorCode.ServiceFailed, errorCode), 0));
                fileActions[fileId].downloadProgressHandler = null;
                if (IsFileActionIsNull(fileActions[fileId]))
                {
                    fileActions.Remove(fileId);
                }
            }
        }
        private void OnThumbnailChanged(string conversationId, string messageId, string contentIndex)
        {
            string fileId = GetFileId(conversationId, messageId, contentIndex);
            if (fileActions.ContainsKey(fileId) && fileActions[fileId].downloadThumbnailAction != null)
            {
                var action = fileActions[fileId].downloadThumbnailAction;
                SparkNet.Image image = m_core_imageService?.getContentThumbnailImage(conversationId, messageId, Convert.ToInt32(contentIndex));

                if (image != null && SaveImageToLocal(image.imageBuffer, action.path))
                {
                    SdkLogger.Instance.Info($"[{fileId}] success download thumbnail to {action.path}");
                    action.completionHandler?.Invoke(new WebexApiEventArgs<string>(true, null, action.path));
                }
                else
                {
                    SdkLogger.Instance.Error($"[{fileId}] save thumbnail failed");
                    action.completionHandler?.Invoke(new WebexApiEventArgs<string>(false, new WebexError(WebexErrorCode.IllegalOperation, "save failed"), action.path));
                }
                fileActions[fileId].downloadThumbnailAction = null;
                if (IsFileActionIsNull(fileActions[fileId]))
                {
                    fileActions.Remove(fileId);
                }
            }
        }
        private void OnThumbnailDownloadFailed(string conversationId, string messageId, string contentIndex, string errorCode)
        {
            string fileId = GetFileId(conversationId, messageId, contentIndex);
            if (fileActions.ContainsKey(fileId) && fileActions[fileId].downloadThumbnailAction != null)
            {
                SdkLogger.Instance.Error($"[{fileId}] download thumbnail failed");
                var action = fileActions[fileId].downloadThumbnailAction;
                action.completionHandler?.Invoke(new WebexApiEventArgs<string>(false, new WebexError(WebexErrorCode.ServiceFailed, errorCode), action.path));
                fileActions[fileId].downloadThumbnailAction = null;
                if (IsFileActionIsNull(fileActions[fileId]))
                {
                    fileActions.Remove(fileId);
                }
            }
        }
        private Message ToMessage(string conversationId, SparkNet.Message input)
        {
            Message m = new Message();

            // Created
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long miliseconds = input.getPublishTime();
            var seconds = miliseconds / 1000;
            dtDateTime = dtDateTime.AddSeconds(seconds);
            m.Created = dtDateTime;

            // Id
            m.Id = StringExtention.EncodeHydraId(StringExtention.HydraIdType.Message, input.getGuid());

            // PersonId, PersonEmail
            m.PersonId = StringExtention.EncodeHydraId(StringExtention.HydraIdType.People, input.getSenderPersonId());
            m.PersonEmail = input.getSenderEmail();

            // SpaceId, SpaceType, ToPersonId, ToPersonEmail
            ToMessageForSpaceTypeAndToPersonIdAndToPersonEmail(ref m, conversationId, input);

            // Text
            if (input.getRichText() != null && input.getRichText().Length > 0)
            {
                m.Text = input.getRichText();
            }
            else
            {
                m.Text = input.getPlainText();
            }

            // Share content
            var contents = input.getShareContents();
            if (contents != null && contents.Length > 0)
            {
                m.Files = new List<RemoteFile>();
                for (int i = 0;i<contents.Length;i++)
                {
                    m.Files.Add(ShareContentToFile(m.SpaceId, m.Id, i, contents[i]));
                }
            }


            // IsSelfMentioned
            m.IsSelfMentioned = input.isSelfMentioned();

            return m;
        }
        private void ToMessageForSpaceTypeAndToPersonIdAndToPersonEmail(ref Message m, string conversationId, SparkNet.Message input)
        {
            // SpaceId, SpaceType
            m.SpaceId = StringExtention.EncodeHydraId(StringExtention.HydraIdType.Space, conversationId);
            m.SpaceType = SpaceType.Direct;
            var conversation = m_core_conversationService?.getConversation(conversationId);
            if (conversation != null)
            {
                m.SpaceType = conversation.isOne2One() ? SpaceType.Direct : SpaceType.Group;
            }

            // ToPersonId, ToPersonEmail
            if (m.SpaceType == SpaceType.Direct)
            {
                string[] participants = conversation?.getParticipants();
                string toPersonId = null;
                if (participants != null && participants.Length == 2)
                {
                    toPersonId = (participants[0] == input.getSenderPersonId()) ? participants[1] : participants[0];
                }
                if (toPersonId != null)
                {
                    var p = conversation.getParticipant(toPersonId);
                    m.ToPersonId = StringExtention.EncodeHydraId(StringExtention.HydraIdType.People, p.getGuid());
                    m.ToPersonEmail = p.getEmail();
                }
            }
        }
        private RemoteFile ShareContentToFile(string spaceId, string messageId, int fileIndex, SparkNet.SharedContent shareContent)
        {
            RemoteFile remoteFile = new RemoteFile
            {
                Name = shareContent.contentDisplayName,
                Mime = shareContent.mime,
                Size = (ulong)shareContent.fileSize,
                RemoteThumbnail = ToThumbnail(shareContent.thumbnail),

                SpaceId = spaceId,
                MessageId = messageId,
                FileIndex = fileIndex
            };

            return remoteFile;
        }

        private RemoteFile.Thumbnail ToThumbnail(SparkNet.Thumbnail thumbnail)
        {
            if (thumbnail == null)
            {
                return null;
            }
            var outThumbnail = new RemoteFile.Thumbnail
            {
                Height = thumbnail.height,
                Width = thumbnail.width,

                Mime = "image/png"//TODO get real mime
            };


            return outThumbnail;

        }
        private List<string> FilterNormalMessages(string spaceId)
        {
            List<string> result = new List<string>();
            var conversation = m_core_conversationService.getConversation(spaceId);
            var originMessages = conversation.getMessages();

            for (int i = 0; i < originMessages.Length; i++)
            {
                var message = conversation.getMessage(originMessages[i]);
                if (message.getMessageType() == SparkNet.MessageType.NormalMessage)
                {
                    result.Add(originMessages[i]);
                }
            }
            return result;
        }
        private bool IsNoMoreFetchAble(string spaceId)
        {
            var conversation = m_core_conversationService.getConversation(spaceId);
            var messages = conversation.getMessages();
            string firstMessageId = messages[0];
            var firstMessage = conversation.getMessage(firstMessageId);

            bool isNoMoreFetchAble = (firstMessage.getMessageType() == SparkNet.MessageType.RoomCreated || firstMessage.getMessageType() == SparkNet.MessageType.NoMoreMessages);

            return isNoMoreFetchAble;

        }

        private bool IsFetchEnough(string conversationId, string mentionedPeople, DateTime? beforetime, string beforeMessageId, int? max, out List<Message> listMsg)
        {
            listMsg = new List<Message>();
            var conversation = m_core_conversationService.getConversation(conversationId);
            bool isNoMoreFetchAble = IsNoMoreFetchAble(conversationId);
            List<string> allMessages = null;


            if (mentionedPeople != null)
            {
                string selfid = "";//TODO, get self personId
                if (mentionedPeople != "me" && mentionedPeople != selfid)
                {
                    SdkLogger.Instance.Error("can only list mentioned caller messages.");
                    return true;
                }
                string[] mentions = m_core_conversationService.getMentions(conversationId);
                allMessages = new List<string>(mentions);
            }
            else
            {
                allMessages = FilterNormalMessages(conversationId);
            }

            int endIndex = allMessages.Count;
            if (beforeMessageId != null)
            {
                for (endIndex = 0; endIndex < allMessages.Count; endIndex++)
                {
                    if (allMessages[endIndex] == beforeMessageId) break;
                }

                if (endIndex == allMessages.Count)
                {
                    SdkLogger.Instance.Error("doesn't find this message by id");
                    return true;
                }
            }
            else if (beforetime != null)
            {
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan diff = beforetime.Value.ToUniversalTime() - origin;

                for (endIndex = 0; endIndex < allMessages.Count; endIndex++)
                {
                    var message = conversation.getMessage(allMessages[endIndex]);
                    if (message.getPublishTime() >= diff.TotalMilliseconds) break;
                }
            }
            else
            {
                SdkLogger.Instance.Debug("both before time and before message are not set");
            }
            SdkLogger.Instance.Debug($"endIndex[{endIndex}]");

            // BeginIndex
            int beginIndex = 0;
            if (max == null)
            {
                if (!isNoMoreFetchAble)
                {
                    return false;
                }
                beginIndex = 0;
            }
            else
            {
                if (max.Value > endIndex && !isNoMoreFetchAble)
                {
                    return false;
                }
                beginIndex = endIndex > max.Value ? endIndex- max.Value : 0;
            }
            SdkLogger.Instance.Debug($"beginIndex[{endIndex}]");

            // Add items in [BeginIndex, EndIndex) to list of Message
            for (int i = beginIndex; i < endIndex; i++)
            {
                listMsg.Add(ToMessage(conversationId, conversation.getMessage(allMessages[i])));
            }

            // the index 0 is the newest message
            listMsg.Reverse();

            return true;
        }

        private string FindExistedConversation(string selfId, string toPersonId)
        {
            string exitedConvId = null;
            var conversations = m_core_conversationService.getConversations();
            foreach (var id in conversations)
            {
                var conv = m_core_conversationService.getConversation(id);
                if (conv.isOne2One())
                {
                    var arrP = conv.getParticipants();
                    if (arrP != null && arrP.Length == 2
                        && ((arrP[0] == selfId && arrP[1] == toPersonId) || (arrP[1] == selfId && arrP[0] == toPersonId)))
                    {
                        exitedConvId = id;
                        break;
                    }
                }
            }
            return exitedConvId;
        }

        private readonly Dictionary<string, Action<string>> creatOne2OneSpaceCompletionHandler = new Dictionary<string, Action<string>>();
        private void CreateOne2OneSpace(string title, string toPersonId, string toEmail, Action<string> completionHandler)
        {
            SdkLogger.Instance.Debug($"create one-on-one conversation. title[{title}] toPersonId[{toPersonId}] toEmail[{toEmail}]");
            string tmpConvId = m_core_conversationService.createOneToOneRoom(title, toPersonId, toEmail, true);
            creatOne2OneSpaceCompletionHandler.Add(tmpConvId, completionHandler);
        }

        private void GetOrCreatOneOnOneSpace(string toPerson, Action<string> conversationId)
        {
            string toPersonId = null;
            string selfId = null;
            if (toPerson == null)
            {
                conversationId.Invoke(null);
                return;
            }
            if (StringExtention.ParseHydraId(toPerson, ref toPersonId) == StringExtention.HydraIdType.People)
            {
                var personClient = new PersonClient(authenticator);
                personClient.GetMe(rsp =>
                {
                    if (!rsp.IsSuccess)
                    {
                        SdkLogger.Instance.Error($"get self id failed.");
                        conversationId.Invoke(null);
                        return;
                    }
                    StringExtention.ParseHydraId(rsp.Data.Id, ref selfId);
                    var exitedConvId = FindExistedConversation(selfId, toPersonId);

                    if (exitedConvId != null)
                    {
                        SdkLogger.Instance.Debug($"this conversation has existed. existedConvId [{exitedConvId}]");
                        conversationId.Invoke(exitedConvId);
                        return;
                    }

                    CreateOne2OneSpace(toPersonId, toPersonId, "", newConvId =>
                    {
                        if (newConvId != null)
                        {
                            SdkLogger.Instance.Debug($"CreateOne2OneSpace success conversation id[{newConvId}]");
                            conversationId.Invoke(newConvId);
                        }
                        else
                        {
                            SdkLogger.Instance.Error("CreateOne2OneSpace failed");
                            conversationId.Invoke(null);
                        }
                    });

                });
            }
            else if (toPerson.Contains("@"))
            {
                var personClient = new PersonClient(authenticator);
                personClient.List(toPerson, null, null, r =>
                {
                    if (r.IsSuccess && r.Data != null && r.Data.Count > 0)
                    {
                        var persons = r.Data;
                        StringExtention.ParseHydraId(persons[0].Id, ref toPersonId);

                        personClient.GetMe(rsp =>
                        {
                            if (!rsp.IsSuccess)
                            {
                                SdkLogger.Instance.Error($"get self id failed.");
                                conversationId.Invoke(null);
                                return;
                            }
                            StringExtention.ParseHydraId(rsp.Data.Id, ref selfId);
                            var exitedConvId = FindExistedConversation(selfId, toPersonId);
                            if (exitedConvId != null)
                            {
                                SdkLogger.Instance.Debug($"this conversation has existed. exitedConvId [{exitedConvId}]");
                                conversationId.Invoke(exitedConvId);
                                return;
                            }

                            CreateOne2OneSpace(toPerson, toPersonId, toPerson, newConvId =>
                            {
                                if (newConvId == null)
                                {
                                    SdkLogger.Instance.Error("CreateOne2OneSpace failed");
                                    conversationId.Invoke(null);
                                    return;
                                }
                                SdkLogger.Instance.Debug($"CreateOne2OneSpace success conversation id[{newConvId}]");
                                conversationId.Invoke(newConvId);
                            });
                        });
                    }
                    else
                    {
                        SdkLogger.Instance.Error($"get to person id failed.");
                        conversationId.Invoke(null);
                    }
                });
            }

        }

        private void Post(string spaceId, string toPerson, string text, List<Mention> mentions, List<LocalFile> files, Action<WebexApiEventArgs<Message>> completionHandler)
        {
            if (spaceId != null)
            {
                string conversationId = null;
                if (StringExtention.ParseHydraId(spaceId, ref conversationId) != StringExtention.HydraIdType.Space)
                {
                    SdkLogger.Instance.Error($"spaceId[{spaceId}] is invailid.");
                    completionHandler?.Invoke(new WebexApiEventArgs<Message>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid spaceId parameter"), null));
                    return;
                }
                SendConversationMsg(conversationId, text, mentions, files, completionHandler);
            }
            else if(toPerson != null)
            {
                GetOrCreatOneOnOneSpace(toPerson, convId =>
                {
                    if (convId == null)
                    {
                        SdkLogger.Instance.Error($"can't get or create this one one one space for toPerson[{toPerson}]");
                        completionHandler?.Invoke(new WebexApiEventArgs<Message>(false, new WebexError(WebexErrorCode.IllegalOperation, "can't get or create this one one one space"), null));
                        return;
                    }
                    SendConversationMsg(convId, text, mentions, files, completionHandler);
                });
            }
        }
        private SparkNet.Image ConvertToThumbNailImage(LocalFile.Thumbnail localThumbnail)
        {
            SparkNet.Image thumbNail = null;
            if (localThumbnail != null)
            {
                try
                {
                    var image = System.Drawing.Image.FromFile(localThumbnail.Path);
                    ImageConverter imgCon = new ImageConverter();
                    byte[] byteArrImage = (byte[])imgCon.ConvertTo(image, typeof(byte[]));
                    thumbNail = new SparkNet.Image(byteArrImage, localThumbnail.Width, localThumbnail.Height, localThumbnail.Mime);
                }
                catch(Exception e)
                {
                    SdkLogger.Instance.Error($"{e.Message}");
                }      
            }
            return thumbNail;
        }
        private SparkNet.MentionInfo[] ToMentionInfoArray(List<Mention> mentions)
        {
            SparkNet.MentionInfo[] arrMentions = null;

            if (mentions != null && mentions.Count > 0)
            {
                arrMentions = new SparkNet.MentionInfo[mentions.Count];
                for (int i = 0; i < mentions.Count; i++)
                {

                    SparkNet.MentionType type = MentionType.Invalid;
                    string personId = null;
                    if (mentions[i] is MentionPerson)
                    {
                        var mention = mentions[i] as MentionPerson;
                        type = MentionType.People;
                        if (StringExtention.ParseHydraId(mention.PersonId, ref personId) != StringExtention.HydraIdType.People)
                        {
                            continue;
                        }
                    }
                    else if (mentions[i] is MentionAll)
                    {
                        type = MentionType.Group;
                    }

                    arrMentions[i] = new SparkNet.MentionInfo(type, 0, 0, false, personId);
                }
            }

            return arrMentions;
        }
        private void SendConversationMsg(string conversationId, string text, List<Mention> mentions, List<LocalFile> files, Action<WebexApiEventArgs<Message>> completionHandler)
        {
            SparkNet.MessageValidationResult result;
            string tempMessageId;

            var conv = m_core_conversationService.getConversation(conversationId);
            if (conv == null || conv.getParticipants().Length == 0)
            {
                SdkLogger.Instance.Error($"conversation id[{conversationId}] is invalid or no participants.");
                completionHandler.Invoke(new WebexApiEventArgs<Message>(false, new WebexError(WebexErrorCode.IllegalOperation, "invalid space id"), null));
                return;
            }

            if (text == null) text = "";

            if (files != null && files.Count > 0)
            {
                SparkNet.FileData[] arrFiles = new SparkNet.FileData[files.Count];
                for (int i = 0;i< files.Count;i++)
                {
                    arrFiles[i] = new SparkNet.FileData(files[i].Path, ConvertToThumbNailImage(files[i].LocalThumbnail));
                }
                result = m_core_conversationService.sendConversationContent(conversationId, text, ToMentionInfoArray(mentions), arrFiles, out tempMessageId);

                for (int i = 0; i < files.Count; i++)
                {
                    string fileId = GetFileId(conversationId, tempMessageId, i);
                    if (fileActions.ContainsKey(fileId))
                    {
                        fileActions[fileId].uploadProgressHandler = files[i].UploadProgressHandler;
                    }
                    else
                    {
                        fileActions.Add(fileId, new FileAction()
                        {
                            uploadProgressHandler = files[i].UploadProgressHandler
                        });
                    }
                }
            }
            else
            {
                result = m_core_conversationService.sendConversationMessage(conversationId, text, ToMentionInfoArray(mentions), out tempMessageId);
            }
            
            if (MessageValidationResult.NoError == result)
            {
                SdkLogger.Instance.Debug($"send message success. tempMessageId[{tempMessageId}]");
                if (messageActions.ContainsKey(tempMessageId))
                {
                    messageActions[tempMessageId].postCompletionHandler = completionHandler;
                }
                else
                {
                    messageActions.Add(tempMessageId, new MessageAction()
                    {
                        postCompletionHandler = completionHandler
                    });
                }
            }
            else
            {
                SdkLogger.Instance.Error($"send message failed for {result.ToString()}");
                completionHandler.Invoke(new WebexApiEventArgs<Message>(false, new WebexError(WebexErrorCode.IllegalOperation, result.ToString()), null));
            }
        }

        private bool SaveImageToLocal(byte[] ImageBuffer, string path)
        {
            var image = ConvertByteArrayToImage(ImageBuffer);
            try
            {
                image.Save(path);
            }
            catch (Exception e)
            {
                SdkLogger.Instance.Error($"save failed. {e.Message}");
                return false;
            }
            return true;
        }
        private System.Drawing.Image ConvertByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream mStream = new MemoryStream(byteArray))
            {
                return System.Drawing.Image.FromStream(mStream);
            }
        }

        private string GetFileId(string conversationId, string messageId, int fileIndex)
        {
            return String.Format($"{conversationId}+{messageId}+{fileIndex}");
        }
        private string GetFileId(string conversationId, string messageId, string fileIndex)
        {
            return String.Format($"{conversationId}+{messageId}+{fileIndex}");
        }
        private bool IsFileActionIsNull(FileAction fileAction)
        {
            return (fileAction == null || (fileAction.downloadProgressHandler == null && fileAction.downloadThumbnailAction == null && fileAction.uploadProgressHandler == null));
        }
        private bool IsMessageActionIsNull(MessageAction messageAction)
        {
            return (messageAction == null || messageAction.deleteCompletionHandler == null);
        }
        private bool IsSessionActionIsNull(SessionAction sessionAction)
        {
            return (sessionAction == null || sessionAction.list == null);
        }

        class SessionAction
        {
            public class ListAction
            {
                public int? listMessageCount;
                public Action<WebexApiEventArgs<List<Message>>> listCompletionHandler;
                public string conversationId;
                public string beforeMessageId;
                public DateTime? before;
                public string mentionedPeople;
            }
            public ListAction list;

        }
        class MessageAction
        {
            public Action<WebexApiEventArgs> deleteCompletionHandler;
            public Action<WebexApiEventArgs<Message>> postCompletionHandler;
        }
        class FileAction
        {
            public class DownloadThumbnailAction
            {
                public string path;
                public Action<WebexApiEventArgs<string>> completionHandler;
            }
            public Action<WebexApiEventArgs<int>> downloadProgressHandler;
            public DownloadThumbnailAction downloadThumbnailAction;
            public Action<WebexApiEventArgs<int>> uploadProgressHandler;
        }

    }
}
