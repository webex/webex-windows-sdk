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

namespace WebexSDK
{
    /// <summary>
    /// Webex object is the entry point to use this Cisco Webex .Net SDK. A Webex object must be created with one of the following Authenticator.
    /// </summary>
    /// <remarks>Since: 0.1.0</remarks>
    public sealed class Webex
    {

        /// <summary>
        /// The version number of this Cisco Webex .Net SDK. 
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public const string Version = "2.0.0";


        /// <summary>
        /// The logger for this SDK.
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public ILogger Logger
        {
            get { return SdkLogger.Instance.Logger; }
            set { SdkLogger.Instance.Logger = value; }
        }

        /// <summary>
        /// Gets or sets log level of the console logging.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public LogLevel ConsoleLogger
        {
            get { return SdkLogger.Instance.Console; }
            set { SdkLogger.Instance.Console = value; }
        }

        /// <summary>
        /// This is the Authenticator object from the application when constructing this Webex object.
        /// It can be used to check and modify authentication state. 
        /// </summary>
        /// <remarks>Since: 0.1.0</remarks>
        public IAuthenticator Authenticator
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the phone, which is an instance of <see cref="Phone"/>.
        /// Phone represents a calling device in Cisco Webex Windows SDK.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public Phone Phone
        {
            get { return Phone.GetInstance(Authenticator); }
        }

        /// <summary>
        /// Gets the spaces, which is an instance of <see cref="SpaceClient"/>
        /// Spaces are virtual meeting places in Cisco Webex where people post messages and collaborate to get work done.
        /// Use spaces to manage the spaces on behalf of the authenticated user.
        /// </summary>
        /// <value>
        /// The spaces.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public SpaceClient Spaces
        {
            get { return new SpaceClient(Authenticator); }
        }

        /// <summary>
        /// Gets the people, which is an instance of <see cref="PersonClient"/>
        /// People are registered users of Cisco Webex.
        /// Use people to find a person on behalf of the authenticated user.
        /// </summary>
        /// <value>
        /// The people.
        /// </value>
        /// <remarks>Since: 0.1.0</remarks>
        public PersonClient People
        {
            get { return new PersonClient(Authenticator); }
        }


        /// <summary>
        /// Gets the memberships, which is an instance of <see cref="MembershipClient"/>
        /// Memberships represent a person's relationships to spaces.
        /// Use membership to manage the authenticated user's relationship to spaces.
        /// </summary>
        /// <value>
        /// The memberships.
        /// </value>
        /// - see: Spaces API about how to manage spaces.
        /// - see: Messages API about how post or otherwise manage the content in a space.
        /// <remarks>Since: 0.1.0</remarks>
        public MembershipClient Memberships
        {
            get { return new MembershipClient(Authenticator); }
        }

        /// <summary>
        /// Gets the messages, which is an instance of <see cref="MessageClient"/>
        /// Messages are how we communicate in a space.
        /// Use messages to manage the messages on behalf of the authenticated user.
        /// </summary>
        /// <value>
        /// The messages.
        /// </value>
        /// - see: Spaces API about how to manage spaces.
        /// - see: Memberships API about how to manage people in a space.
        /// <remarks>Since: 0.1.0</remarks>
        public MessageClient Messages
        {
            get { return MessageClient.GetInstance(Authenticator); }
        }

        /// <summary>
        /// Gets the webhooks.
        /// Webhooks allow the application to be notified via HTTP(or HTTPS) when a specific event occurs in Cisco Webex
        /// </summary>
        /// <value>
        /// The webhooks.
        /// </value>
        /// <example>a new message is posted into a specific space.</example>
        /// Use Webhooks to create and manage the webhooks for specific events.
        /// <remarks>Since: 0.1.0</remarks>
        public WebhookClient Webhooks
        {
            get { return new WebhookClient(Authenticator); }
        }

        /// <summary>
        /// Gets the teams, which is an instance of <see cref="TeamClient"/>
        /// Teams are groups of people with a set of spaces that are visible to all members of that team.
        /// Use teams to create and manage the teams on behalf of the authenticated user.
        /// </summary>
        /// <value>
        /// The teams.
        /// </value>
        /// - see: Team Memberships API about how to manage people in a team.
        /// - see: Memberships API about how to manage people in a space.
        /// <remarks>Since: 0.1.0</remarks>
        public TeamClient Teams
        {
            get { return new TeamClient(Authenticator); }
        }

        /// <summary>
        /// Gets the team memberships, which is an instance of <see cref="TeamMembershipClient"/>
        /// Team Memberships represent a person's relationships to teams.
        /// Use teamMemberships to create and manage the team membership on behalf of the authenticated user.
        /// </summary>
        /// <value>
        /// The team memberships.
        /// </value>
        /// - see: Teams API about how to manage teams.
        /// - see: Spaces API about how to manage spaces.
        /// <remarks>Since: 0.1.0</remarks>
        public TeamMembershipClient TeamMemberships
        {
            get { return new TeamMembershipClient(Authenticator); }
        }

        readonly SCFCore m_core;

        /// <summary>
        /// Initializes a new instance of the <see cref="Webex"/>,with an Authenticator class.
        /// </summary>
        /// <param name="authenticator">The authenticator strategy for this SDK</param>
        /// <remarks>Since: 0.1.0</remarks>
        public Webex(IAuthenticator authenticator)
        {
            this.Authenticator = authenticator;
            m_core =  SCFCore.Instance;
        }

    }

    internal sealed class SCFCore
    {
        private static volatile SCFCore instance = null;
        private static readonly object lockHelper = new object();

        public SparkNet.CoreFramework m_core;
        public SparkNet.TelephonyService m_core_telephoneService;
        public SparkNet.ConversationService m_core_conversationService;
        public SparkNet.DeviceManager m_core_deviceManager;
        public SparkNet.ImageService m_core_imageService;
        private SCFCore()
        {
            Console.WriteLine("scf core init");
            m_core = new SparkNet.CoreFramework();
            string strCurUserLogDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            strCurUserLogDir += "\\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + "\\";
            m_core.configureLog(strCurUserLogDir);
            m_core.init(Webex.Version, UserAgent.Instance.OSVersion, UserAgent.Instance.OSLanguage, "", strCurUserLogDir, UserAgent.Instance.Name);
            m_core_telephoneService = m_core.getTelephonyService();
            m_core_conversationService = m_core.getConversationService();
            m_core_deviceManager = m_core.getDeviceManager();
            m_core_imageService = m_core.getImageService();
        }
        public static SCFCore Instance
        {
            get
            {
                if (null == instance)
                {
                    lock (lockHelper)
                    {
                        if (null == instance)
                        {
                            instance = new SCFCore();
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

            Phone.GetInstance(null).UnRegisterToCore();
            MessageClient.GetInstance(null).UnRegisterToCore();
            m_core.exit();
            m_core.Dispose();
            instance = null;
        }
    }
}
