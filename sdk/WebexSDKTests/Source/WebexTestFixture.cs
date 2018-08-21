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
using System.Threading;
using RestSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;

namespace WebexSDK.Tests
{

    [TestClass]
    public class AssemblyFixture
    {
        private static readonly string testFixtureApp = "TestFixtureApp";

        [AssemblyInitialize]
        public static void AssemblySetup(TestContext context)
        {
            Console.WriteLine("Assembly Initialize.");
            var fixture = WebexTestFixture.Instance;

            MessageHelper.Init();
            MessageHelper.CloseTestFixtureApp(testFixtureApp);
            Thread.Sleep(50000);
        }

        [AssemblyCleanup]
        public static void AssemblyTeardown()
        {
            Console.WriteLine("Assembly Cleanup.");
            MessageHelper.CloseTestFixtureApp(testFixtureApp);
            WebexTestFixture.Instance.UnLoad();
            Thread.Sleep(15000);
        }
    }

    public class TestUser
    {
        public string AccessToken { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string OrgId { get; set; }
        public string PersonId { get; set; }
    }

    public class WebexTestFixture
    {
        private static volatile WebexTestFixture instance;
        private static readonly object lockHelper = new object();

        public Webex webex;
        public Webex jwtWebex;

        private JWTAuthenticator jwtAuth;
        private string adminClientId;
        private string adminClientSecret;
        private string adminAccessToken;
        //public WebexSDK.Webex webex;
        public TestUser selfUser;
        public Phone phone;

        class WebexTestFixtureAuth : WebexSDK.IAuthenticator
        {
            private string token;

            public WebexTestFixtureAuth(string token)
            {
                this.token = token;
            }
            public void Authorized(Action<WebexApiEventArgs> completionHandler)
            {
                completionHandler(new WebexApiEventArgs(true, null));
            }
            public void Deauthorize()
            {
            }
            public void AccessToken(Action<WebexSDK.WebexApiEventArgs<string>> completionHandler)
            {
                completionHandler(new WebexSDK.WebexApiEventArgs<string>(true, null, token));
            }
            public void RefreshToken(Action<WebexApiEventArgs<string>> completionHandler)
            {
                completionHandler(new WebexSDK.WebexApiEventArgs<string>(true, null, token));
            }
        }

        class AccessToken
        {
            public int Expires_in { get; set; }
            public string Token_type { get; set; }
            public string Access_token { get; set; }
        }


        public WebexTestFixture()
        {
            adminClientId = ConfigurationManager.AppSettings["AdminClientID"] ?? "";
            adminClientSecret = ConfigurationManager.AppSettings["AdminSecret"] ?? "";

            // get access token
            adminAccessToken = CreateAdminAccessToken(adminClientId, adminClientSecret);
            if (adminAccessToken == null)
            {
                Console.WriteLine("Error: create access token failed!");
                return;
            }

            //create the first user
            selfUser = CreateUser(adminAccessToken, adminClientId, adminClientSecret);
            if (selfUser == null)
            {
                Console.WriteLine("Error: create test user failed!");
                return;
            }


            webex = CreateWebex();

            Console.WriteLine("WebexTestFixture build success!");
        }

        public Webex CreateWebex()
        {
            if (webex == null)
            {
                webex = new WebexSDK.Webex(new WebexTestFixtureAuth(selfUser.AccessToken));
            }

            return webex;
        }

        public Webex CreateWebexbyJwt()
        {
            if (jwtWebex == null)
            {
                jwtAuth = new JWTAuthenticator();
                jwtWebex = new WebexSDK.Webex(jwtAuth);

                //login
                for (int count = 1; count <= 5; count++)
                {
                    if (JWtLogin() == true)
                    {
                        Console.WriteLine("JWtLogin success.");
                        break;
                    }
                    Console.WriteLine($"Error: jwt login fail[{count}].");

                    if (count == 5)
                    {
                        jwtWebex = null;
                        return null;
                    }
                }
            }

            return jwtWebex;
        }


        private static string CreateAdminAccessToken(string clientId, string clientSecret)
        {
            RestRequest request = new RestSharp.RestRequest(Method.POST);

            byte[] encodedByte = Encoding.UTF8.GetBytes(string.Format("{0}:{1}", clientId, clientSecret));
            string adminCredentials = Convert.ToBase64String(encodedByte);

            request.AddHeader("Authorization", "Basic " + adminCredentials);
            request.AddParameter("grant_type", "client_credentials", ParameterType.GetOrPost);
            request.AddParameter("scope", "webexsquare:admin Identity:SCIM", ParameterType.GetOrPost);

            var client = new RestClient();
            client.BaseUrl = new System.Uri("https://idbroker.webex.com/idb/oauth2/v1/access_token");

            var response = client.Execute<AccessToken>(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK
            && response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                Console.WriteLine($"Error: create access token response: {response.StatusDescription}");
                return null;
            }

            return response.Data.Access_token;
        }

        private static TestUser CreateUser(string adminAccessToken, string adminClientId, string adminClientSecret)
        {
            string[] entitlements = { "spark", "webExSquared", "squaredCallInitiation", "squaredTeamMember", "squaredRoomModeration" };
            string scopes = "spark:people_read spark:rooms_read spark:rooms_write spark:memberships_read spark:memberships_write spark:messages_read spark:messages_write spark:teams_write spark:teams_read spark:team_memberships_write spark:team_memberships_read";
            string userName = Guid.NewGuid().ToString();
            string email = userName + "@squared.example.com";

            RestRequest request = new RestSharp.RestRequest(Method.POST);
            request.AddHeader("Authorization", "Bearer " + adminAccessToken);
            request.AddJsonBody(new
            {
                clientId = adminClientId,
                clientSecret = adminClientSecret,
                emailTemplate = email,
                displayName = userName,
                password = "P@ssw0rd123",
                entitlements = entitlements,
                authCodeOnly = "false",
                scopes = scopes,
            });

            //Cisco Webex platform is dropping support for TLS 1.0 as of March 16, 2018
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            var client = new RestClient();
            client.BaseUrl = new System.Uri("https://conv-a.wbx2.com/conversation/api/v1/users/test_users_s");

            var response = client.Execute<WebexUser>(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK
            && response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                Console.WriteLine($"Error: createUser response: {response.StatusCode} {response.StatusDescription}");
                return null;
            }

            return new TestUser
            {
                AccessToken = response.Data.token.access_token,
                Email = response.Data.user.email,
                Name = response.Data.user.name,
                OrgId = response.Data.user.orgId,
                PersonId = GetPersonIdFromUserId(response.Data.user.id),
            };
        }

        public static string GetPersonIdFromUserId(string userId)
        {
            byte[] encodedByte = Encoding.UTF8.GetBytes("ciscospark://us/PEOPLE/" + userId);
            return Convert.ToBase64String(encodedByte).Replace("=", "");
        }

        

        public TestUser CreatUser()
        {
            return CreateUser(adminAccessToken, adminClientId, adminClientSecret);
        }

        public Space CreateSpace(string title)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Space>();
            webex.Spaces.Create(title, null, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess)
            {
                return response.Data;
            }

            return null;
        }

        public bool DeleteSpace(string spaceId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
            webex.Spaces.Delete(spaceId, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                return false;
            }

            if (response.IsSuccess)
            {
                return true;
            }

            return false;
        }

        public Membership CreateMembership(string spaceId, string email, string personId, bool isModerator)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Membership>();
            if (email != null)
            {
                webex.Memberships.CreateByPersonEmail(spaceId, email, isModerator, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }
            else
            {
                webex.Memberships.CreateByPersonId(spaceId, personId, isModerator, rsp =>
                {
                    response = rsp;
                    completion.Set();
                });
            }


            if (!completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess)
            {
                return response.Data;
            }

            return null;
        }

        public Team CreateTeam(string teamName)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Team>();
            webex.Teams.Create(teamName, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess)
            {
                return response.Data;
            }

            return null;
        }

        public bool DeleteTeam(string teamId)
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();
            webex.Teams.Delete(teamId, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                return false;
            }

            if (response.IsSuccess)
            {
                return true;
            }

            return false;
        }

        public static WebexTestFixture Instance
        {
            get
            {
                if (null == instance)
                {
                    lock (lockHelper)
                    {
                        if (null == instance)
                        {
                            instance = new WebexTestFixture();
                        }
                    }

                }
                return instance;
            }
        }

        public void UnLoad()
        {
            if (instance == null)
            {
                return;
            }
            if (webex != null)
            {
                webex.Authenticator.Deauthorize();
            }
            if (jwtWebex != null)
            {
                jwtWebex.Authenticator.Deauthorize();
            }
            
            instance = null;
            Console.WriteLine("fixture unloaded");
        }


        private bool JWtLogin()
        {
            string jwt = ConfigurationManager.AppSettings["JWT"] ?? "";
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs();

            jwtAuth.AuthorizeWith(jwt, rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                Console.WriteLine("authorizeWith timeout");
                return false;
            }

            return response.IsSuccess;
        }

        

        private bool OAuthLogin()
        {
            return false;
        }

        private Person GetMe()
        {
            var completion = new ManualResetEvent(false);
            var response = new WebexApiEventArgs<Person>();
            webex.People.GetMe(rsp =>
            {
                response = rsp;
                completion.Set();
            });

            if (!completion.WaitOne(30000))
            {
                return null;
            }

            if (response.IsSuccess)
            {
                return response.Data;
            }
            return null;
        }
    }








    public class Locale
    {
        public string languageCode { get; set; }
        public string countryCode { get; set; }
    }

    public class SipAddresses
    {
        public string type { get; set; }
        public string value { get; set; }
        public string primary { get; set; }
    }

    public class UserSettings
    {
        public string sparkSignUpDate { get; set; }
    }


    public class User
    {
        public string id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string givenName { get; set; }
        public string type { get; set; }
        public List<string> entitlements { get; set; }
        public List<string> roles { get; set; }
        public List<string> photos { get; set; }
        public List<string> ims { get; set; }
        public List<string> phoneNumbers { get; set; }
        public string orgId { get; set; }
        public string isPartner { get; set; }
        public Locale locale { get; set; }
        public List<SipAddresses> sipAddresses { get; set; }
        public UserSettings userSettings { get; set; }
        public List<string> accountStatus { get; set; }
    }

    public class Token
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string refresh_token_expires_in { get; set; }
    }

    public class WebexUser
    {
        public User user { get; set; }
        public string authorization { get; set; }
        public Token token { get; set; }
    }

    class TimerHelper
    {
        public static System.Timers.Timer StartTimer(int interval, System.Timers.ElapsedEventHandler timeOutCallback)
        {
            System.Timers.Timer t = new System.Timers.Timer(interval);
            t.Elapsed += timeOutCallback;
            t.AutoReset = false;
            t.Enabled = true;

            return t;
        }
    }

    public class StringExtention
    {
        public static string Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 1: output += "==="; break; // Three pad chars
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder

            return System.Text.Encoding.UTF8.GetString(converted);
        }

        public static string Base64UrlEncode(string input)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return System.Convert.ToBase64String(plainTextBytes).Replace("=", "").Replace('+', '-').Replace('/', '_'); ;
        }
        public enum HydraIdType
        {
            Error,
            People,
            Space,
            Message,
            Unknow,
        }
        public static HydraIdType GetHydraIdType(string address)
        {
            string peopleUrl = "ciscospark://us/PEOPLE/";
            string spaceUrl = "ciscospark://us/ROOM/";
            string messageUrl = "ciscospark://us/MESSAGE/";

            HydraIdType result = HydraIdType.Error;

            try
            {
                var decodedStr = StringExtention.Base64UrlDecode(address);
                if (decodedStr.StartsWith(peopleUrl))
                {
                    result = HydraIdType.People;
                }
                else if (decodedStr.StartsWith(spaceUrl))
                {
                    result = HydraIdType.Space;
                }
                else if (decodedStr.StartsWith(messageUrl))
                {
                    result = HydraIdType.Message;
                }
                else
                {
                    result = HydraIdType.Unknow;
                }
            }
            catch
            {
                result = HydraIdType.Error;
            }


            return result;
        }
    }

    public class MessageHelper
    {
        private static WebexSDKTests.ServiceReference.TestFixtureServiceClient proxy1;
        private static WebexSDKTests.ServiceReference.TestFixtureServiceClient proxy2;

        public static void Init()
        {
            if (proxy1 == null)
            {
                proxy1 = new WebexSDKTests.ServiceReference.TestFixtureServiceClient("TestFixtureService1");
            }
            if (proxy1.State != System.ServiceModel.CommunicationState.Opened || proxy1.State != System.ServiceModel.CommunicationState.Opening)
            {
                proxy1.Open();
            }
            if (proxy2 == null)
            {
                proxy2 = new WebexSDKTests.ServiceReference.TestFixtureServiceClient("TestFixtureService1");
            }
            if (proxy2.State != System.ServiceModel.CommunicationState.Opened || proxy2.State != System.ServiceModel.CommunicationState.Opening)
            {
                proxy2.Open();
            }
        }


        static bool breakLoopSignal = false;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PeekMessage(
           ref MSG lpMsg,
           Int32 hwnd,
           Int32 wMsgFilterMin,
           Int32 wMsgFilterMax,
           PeekMessageOption wRemoveMsg);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool TranslateMessage(ref MSG lpMsg);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern Int32 DispatchMessage(ref MSG lpMsg);

        private enum PeekMessageOption
        {
            PM_NOREMOVE = 0,
            PM_REMOVE
        }
        public const int WM_QUIT = 0x0012;
        public const int WM_COPYDATA = 0x004A;


        [StructLayout(LayoutKind.Sequential)]
        public struct CopyDataStruct
        {
            public IntPtr dwData;
            public int cbData;

            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }


        public static void RunDispatcherLoop()
        {
            MSG msg = new MSG();
            // max loop time 2 minute
            var t = TimerHelper.StartTimer(120000, (o, e) =>
            {
                breakLoopSignal = true;
            });

            while (true)
            {
                if (PeekMessage(ref msg, 0, 0, 0, PeekMessageOption.PM_REMOVE))
                {
                    if (msg.message == WM_QUIT)
                    {
                        Console.WriteLine("break loop");
                        break;
                    }

                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }

                if (breakLoopSignal)
                {
                    breakLoopSignal = false;
                    t.Stop();
                    t.Close();
                    break;
                }
            }
        }

        public static void BreakLoop()
        {
            breakLoopSignal = true;
        }

        public static void SendMessage(string windowName, string strMsg)
        {
            StackTrace st = new StackTrace(true);
            StackFrame sf = st.GetFrame(2);
            if (windowName == "thirdpart")
            {
                MessageHelper.proxy2.SendCommandMsg(sf.GetMethod().Name + ":" + strMsg);
            }
            else
            {
                MessageHelper.proxy1.SendCommandMsg(sf.GetMethod().Name + ":" + strMsg);
            }
            Thread.Sleep(500);
        }

        public static void SetTestMode_CalleeAutoAnswerAndHangupAfter30Seconds(string windowName)
        {
            MessageHelper.SendMessage(windowName, "Enable");
            MessageHelper.SendMessage(windowName, "AutoAnswer");
            MessageHelper.SendMessage(windowName, "ConversationTimer:30000");
        }

        public static void SetTestMode_CalleeAutoDecline(string windowName)
        {
            MessageHelper.SendMessage(windowName, "Enable");
            MessageHelper.SendMessage(windowName, "AutoDecline");
        }

        public static void SetTestMode_CalleeAutoAnswerAndMuteVideoAndHangupAfter30Seconds(string windowName)
        {
            MessageHelper.SendMessage(windowName, "Enable");
            MessageHelper.SendMessage(windowName, "AutoAnswer");
            MessageHelper.SendMessage(windowName, "MuteVideo");
            MessageHelper.SendMessage(windowName, "ConversationTimer:100000");
        }

        public static void SetTestMode_CalleeAutoAnswerAndMuteVideoAndUnMuteVideoAndHangupAfter30Seconds(string windowName)
        {
            MessageHelper.SendMessage(windowName, "Enable");
            MessageHelper.SendMessage(windowName, "AutoAnswer");
            MessageHelper.SendMessage(windowName, "MuteVideo:10000");
            MessageHelper.SendMessage(windowName, "ConversationTimer:100000");
        }

        public static void SetTestMode_CalleeAutoAnswerAndMuteAudioAndHangupAfter30Seconds(string windowName)
        {
            MessageHelper.SendMessage(windowName, "Enable");
            MessageHelper.SendMessage(windowName, "AutoAnswer");
            MessageHelper.SendMessage(windowName, "MuteAudio");
            MessageHelper.SendMessage(windowName, "ConversationTimer:100000");
        }

        public static void SetTestMode_CalleeAutoAnswerAndMuteAudioAndUnMuteAudioAndHangupAfter30Seconds(string windowName)
        {
            MessageHelper.SendMessage(windowName, "Enable");
            MessageHelper.SendMessage(windowName, "AutoAnswer");
            MessageHelper.SendMessage(windowName, "MuteAudio:5000");
            MessageHelper.SendMessage(windowName, "ConversationTimer:100000");
        }

        public static void SetTestMode_CalleeAutoAnswerAndStartShareAndHangupAfter30s(string windowName)
        {
            MessageHelper.SendMessage(windowName, "Enable");
            MessageHelper.SendMessage(windowName, "AutoAnswer");
            MessageHelper.SendMessage(windowName, "StartShare");
            MessageHelper.SendMessage(windowName, "ConversationTimer:30000");
        }

        public static void SetTestMode_CalleeAutoAnswerAndStartShare15sAndHangupAfter30s(string windowName)
        {
            MessageHelper.SendMessage(windowName, "Enable");
            MessageHelper.SendMessage(windowName, "AutoAnswer");
            MessageHelper.SendMessage(windowName, "StartShare:10000");
            MessageHelper.SendMessage(windowName, "ConversationTimer:30000");
        }

        public static void SetTestMode_RemoteDialout(string windowName, string address)
        {
            MessageHelper.SendMessage(windowName, "Enable");
            MessageHelper.SendMessage(windowName, "Dial:" + address);
        }

        // Message
        public static void SetTestMode_RemoteSendDirectMessage(string windowName, string address, string text)
        {
            MessageHelper.SendMessage(windowName, "SendDirectMessage:" + address + ":" + text);
        }
        public static void SetTestMode_RemoteSendDirectMessageWithFiles(string windowName, string address, string text)
        {
            MessageHelper.SendMessage(windowName, "SendDirectMessageWithFiles:" + address + ":" + text);
        }
        public static void SetTestMode_RemoteSendSpaceMessage(string windowName, string spaceId, string text)
        {
            MessageHelper.SendMessage(windowName, "SendSpaceMessage:" + spaceId + ":" + text);
        }
        public static void SetTestMode_RemoteSendSpaceMessageWithMention(string windowName, string spaceId, string text, string mentioned)
        {
            MessageHelper.SendMessage(windowName, "SendSpaceMessage:" + spaceId + ":" + text+ ":" + mentioned);
        }

        public static void CloseTestFixtureApp(string windowName)
        {
            //MessageHelper.SendMessage(windowName, "CloseApp");
        }
    }

}
