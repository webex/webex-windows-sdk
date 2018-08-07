# Cisco Webex Windows SDK

[![NuGet version (Cisco.Webex.WindowsSDK)](https://img.shields.io/nuget/v/Cisco.Webex.WindowsSDK.svg?style=flat-square)](https://www.nuget.org/packages/Cisco.Webex.WindowsSDK/)
[![Build status](https://ci.appveyor.com/api/projects/status/k7bu14htnvcxkdwd/branch/master?svg=true)](https://ci.appveyor.com/project/ciscowebex/webex-windows-sdk/branch/master)
[![LICENSE](https://img.shields.io/github/license/webex/webex-windows-sdk.svg)](https://github.com/webex/webex-windows-sdk/blob/master/LICENSE)
> The Cisco Webex™ Windows SDK
 
The Cisco Webex Windows SDK makes it easy to integrate secure and convenient Cisco Webex calling and messaging features in your Windows applications.

This SDK is built with **Vistual Studio 2017** and requires:

- .NET Framework 4.5.2 or higher version
- Win8 or Win10

## Table of Contents
- [Install](#install)
- [Usage](#usage)
- [Contribute](#contribute)
- [License](#license)

## Install
In your Windows application for example WPF project, here are steps to integrate the Cisco Webex Windows SDK into your project:

1. Right click your project, and select "Manage NuGet Packages..."  
2. Search "Cisco.Webex.WindowsSDK" in the Browse tag  
3. Install the lastest stable version

## Usage
To use the SDK, you will need Cisco Webex integration credentials. If you do not already have a Cisco Webex account, visit the [Cisco Webex for Developers portal](https://developer.webex.com/) to create your account and [register an integration](https://developer.webex.com/authentication.html#registering-your-integration). Your app will need to authenticate users via an [OAuth](https://oauth.net/) grant flow for existing Cisco Webex users or a [JSON Web Token](https://jwt.io/) for guest users without a Cisco Webex account.

See the [Windows SDK area](https://developer.webex.com/sdk-for-windows.html) of the Cisco Webex for Developers site for more information about this SDK.

### Examples
Here are some examples of how to use the Windows SDK in your application. More details can be found under [Windows SDK Demo app](https://github.com/webex/webex-windows-sdk-example).

1. Create a new *Webex* instance using Webex ID authentication ([OAuth](https://oauth.net/)-based):  

    ``` c# 
    string clientId = "your client id";  
    string clientSecret = "your client secret";
    string redirectUri = "KitchenSink://response/";
    string scope = "webex:all";
    var auth = new OAuthAuthenticator(clientId, clientSecret, scope, redirectUri);
    // authCode(64 bits) can be extracted from url by loading auth.authorizationUrl with a WebBrowser
    var webex = new Webex(auth);
    auth?.Authorize(authCode, result =>
    {
        if (result.Success)
        {
            System.Console.WriteLine("authorize success!");
        }
        else
        {
            System.Console.WriteLine("authorize failed!");
        }
    });
    ```

2. Create a new *Webex* instance using Guest ID authentication ([JWT](https://jwt.io/)-based):  

    ```c#
    var auth = new JWTAuthenticator();
    var webex = new Webex(auth);
    auth?.AuthorizeWith(jwt, result =>
    {
        if (result.Success)
        {
            System.Console.WriteLine("authorize success!");
        }
        else
        {
            System.Console.WriteLine("authorize failed!");
        }
    });
    
    ```

3. Register the device to make or receive calls:  
 
    ``` c#
    webex?.Phone.Register(result =>
    {
        if (result.Success == true)
        {
            System.Console.WriteLine("webex cloud connected");
        }
        else
        {
            System.Console.WriteLine("webex cloud connect failed");
        }
    });
    ```
    
4. Make an outgoing call:  

    ```c#
    // dial
    // calleeAddress can be email address, person ID, or a room ID
    webex?.Phone.Dial(calleeAddress, MediaOption.AudioVideoShare(curCallView.LocalViewHandle, curCallView.RemoteViewHandle, curCallView.RemoteShareViewHandle), result =>
    {
        if (result.Success)
        {
            currentCall = result.Data;
            RegisterCallEvent();
        }
        else
        {
            System.Console.WriteLine($"Error: {result.Error?.errorCode.ToString()} {result.Error?.reason}");
        }
    });
    
    // register call event handlers
    void RegisterCallEvent()
    {
        currentCall.onRinging += CurrentCall_onRinging;
        currentCall.onConnected += CurrentCall_onConnected;
        currentCall.onDisconnected += CurrentCall_onDisconnected;
        currentCall.onMediaChanged += CurrentCall_onMediaChanged;
        currentCall.onCapabilitiesChanged += CurrentCall_onCapabilitiesChanged;
        currentCall.onCallMembershipChanged += CurrentCall_onCallMembershipChanged;    
    }
    
    // when video window such as local/remote/sharing window is resized or hided, call corresponding updateView with the windows handle
    currentCall.UpdateLocalView(curCallView.LocalViewHandle);
    ```

5. Answer incoming call:

    ```c#
    // register incoming call event
    webex?.Phone.OnIncoming += Phone_onIncoming;
    
    // get call object
    void Phone_onIncoming(WebexSDK.Call obj)
    {
        currentCall = obj;
    }
    
    // register call event handler and answer the call
    RegisterCallEvent();
    
    // answer current call  
    currentCall?.Answer(MediaOption.audioVideo(curCallView.LocalViewHandle, curCallView.RemoteViewHandle), result =>
    {
        if (!result.Success)
        {
            System.Console.WriteLine($"Error: {result.Error?.errorCode.ToString()} {result.Error?.reason}");
        }
    });
    
    ```

6. Start a screen share
    ```c#
    // Fetch all shareable desktop sources
    this.currentCall.FetchShareSources(ShareSourceType.Desktop, result =>
    {
        if (result.IsSuccess)
        {
            List<ShareSource> ShareSourceList = result.Data;
        }
    });
    
    // Fetch all shareable application sources
    this.currentCall.FetchShareSources(ShareSourceType.Application, result =>
    {
        if (result.IsSuccess)
        {
            List<ShareSource> ShareSourceList = result.Data;
        }
    });
    
    // Start share a selected share source.
    this.currentCall.StartShare(sourceId, r =>
    {
        if (r.IsSuccess)
        {
            System.Console.WriteLine("Start share success.");
        }
        else
        {
            System.Console.WriteLine($"Start share failed! Error: {r.Error?.ErrorCode.ToString()} {r.Error?.Reason}");
        }
    });
    ```

7. Receive a screen share
    ```c#
    // set share view handle when invoke dial method.
    webex?.Phone.Dial(calleeAddress, MediaOption.AudioVideoShare(curCallView.LocalViewHandle, curCallView.RemoteViewHandle, curCallView.RemoteShareViewHandle), result =>
    {});
    
    // or set set share view handle when receive RemoteSendingShareEvent
    currentCall.OnMediaChanged += CurrentCall_onMediaChanged;
    private void CurrentCall_onMediaChanged(MediaChangedEvent mediaChgEvent)
    {
        if (mediaChgEvent is RemoteSendingShareEvent)
        {
            var remoteSendingShareEvent = mediaChgEvent as RemoteSendingShareEvent;
            if (remoteSendingShareEvent.IsSending)
            {
                currentCall.SetRemoteShareView(curCallView.RemoteShareViewHandle);
            }
        }
    }
    ```

8. Create a new Cisco Webex space, add a user to the space:

    ```c#
    // Create a Cisco Webex room:
    WebexSDK.Room room = null;
    webex?.Rooms.Create("hello world", null, rsp =>
    {
        if (rsp.Success){
            room = rsp.Data;
            System.Console.WriteLine("create space successfully");
        }
    });
    
    // Add a user to the room
    webex?.Memberships.CreateByPersonEmail(room?.Id, "email address", false, rsp =>
    {
        if (rsp.Success)
        {
            System.Console.WriteLine("add user successfully");
        }
    });
    
    // send message to the room
    webex?.Messages.PostToRoom(room?.Id, "hello", null, rsp =>
    {
        if(rsp.Success)
        {
            System.Console.WriteLine("post message successfully");
        }
    });
    
    ```
9. Post a message
    ```c#
    // Post a message to a person by email or person ID.
    webex?.Messages.PostToPerson(toPerson, text, files, r =>
    {
        if (r.IsSuccess)
        {
            Message message = r.Data;
            System.Console.WriteLine($"{message.PersonEmail} {message.Created}");
            System.Console.WriteLine($"{message.Text}");
        }
        else
        {
            System.Console.WriteLine($"send the message failed. {r.Error.ErrorCode} {r.Error.Reason}");
        }
    });
    
    // Post a message to a room by roomId.
    webex?.Messages.PostToRoom(roomId, text, mentions, files, r =>
    {
        if (r.IsSuccess)
        {
            Message message = r.Data;
            System.Console.WriteLine($"{message.PersonEmail} {message.Created}");
            System.Console.WriteLine($"{message.Text}");
        }
        else
        {
            System.Console.WriteLine($"send the message failed. {r.Error.ErrorCode} {r.Error.Reason}");
        }
    });
    ```
    
10. Mention 
    ```c#
    // Mention list
    List<Mention> Mentions = new List<Mention>();
    
    // Mention All
    Mentions.Add(new MentionAll());
    webex?.Messages.PostToRoom(roomId, text, Mentions, files, r =>
    {
        if (r.IsSuccess)
        {
            Message message = r.Data;
            System.Console.WriteLine($"{message.PersonEmail} {message.Created}");
            System.Console.WriteLine($"{message.Text}");
        }
        else
        {
            System.Console.WriteLine($"send the message failed. {r.Error.ErrorCode} {r.Error.Reason}");
        }
    });
    
    // Mention one person
    Mentions.Add(new MentionPerson(personId));
    webex?.Messages.PostToRoom(roomId, text, Mentions, files, r =>
    {
        if (r.IsSuccess)
        {
            Message message = r.Data;
            System.Console.WriteLine($"{message.PersonEmail} {message.Created}");
            System.Console.WriteLine($"{message.Text}");
        }
        else
        {
            System.Console.WriteLine($"send the message failed. {r.Error.ErrorCode} {r.Error.Reason}");
        }
    });
    
    // Receive a Mention: See receive a message.
    
    ```
11. Receive a message 
    ```c#
    webex?.Messages.OnEvent += OnMessageEvent;
    private void OnMessageEvent(MessageEvent e)
    {
        if (e is MessageArrived)
        {
            System.Console.WriteLine("received a message.");
            var messageArrived = e as MessageArrived;
            var msgInfo = messageArrived?.Message;
            if (msgInfo != null)
            {
                // self is mentioned
                if (msgInfo.IsSelfMentioned)
                {
                    System.Console.WriteLine($"{msgInfo.PersonEmail} mentioned you.");
                }

                // message text
                System.Console.WriteLine($"{msgInfo.PersonEmail} {msgInfo.Created}");
                System.Console.WriteLine($"{msgInfo.Text}");

                // received attached files.
                if (msgInfo.Files != null && msgInfo.Files.Count > 0)
                {
                    foreach (var file in msgInfo.Files)
                    {
                        // download thumbnail if exist.
                        if (file.RemoteThumbnail != null)
                        {
                            webex?.Messages.DownloadThumbnail(file, to, r =>
                            {
                                if (r.IsSuccess)
                                {
                                    // callback download path.
                                    ThumbnailPath = r.Data;
                                }
                            });
                        }
                        // download file
                        webex?.Messages.DownloadFile(file, downloadPath, r =>
                        {
                            if (r.IsSuccess)
                            {
                                System.Console.WriteLine($"downloading {r.Data}%");
                            }
                            else
                            {
                                System.Console.WriteLine($"download failed {r.Data}");
                            }
                        });
                    }
                }
            }
        }
    }
    ```

## Contribute

Pull requests welcome. To suggest changes to the SDK, please fork this repository and submit a pull request with your changes. Your request will be reviewed by one of the project maintainers.

## License

&copy; 2016-2018 Cisco Systems, Inc. and/or its affiliates. All Rights Reserved.

See [LICENSE](https://github.com/webex/webex-windows-sdk/blob/master/LICENSE) for details.
