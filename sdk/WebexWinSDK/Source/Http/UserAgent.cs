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
using System.Management;

namespace WebexSDK
{
    internal class UserAgent
    {
        private static volatile UserAgent instance = null;
        private static readonly object lockHelper = new object();

        public string OSVersion { get; set; }
        public string OSLanguage { get; set; }
        public string OSArchitecture { get; set; }

        private UserAgent()
        {
            string OSBuildNumber = GetOSInfo("Version");
            OSVersion = "Microsoft Windows " + OSBuildNumber;
            OSArchitecture = GetOSArchitecture();
            OSLanguage = GetOSLanguage();
        }

        public static UserAgent Instance
        {
            get
            {
                if (null == instance)
                {
                    lock (lockHelper)
                    {
                        if (null == instance)
                        {
                            instance = new UserAgent();
                        }
                    }
                }
                return instance;
            }
        }

        public string Name
        {
            get
            {
                return string.Format($"webex_win_sdk/{Webex.Version} ({OSVersion}; {OSArchitecture})");
            } 
        }
        public static string GetOSLanguage()
        {
            string s = System.Globalization.CultureInfo.InstalledUICulture.NativeName;
            return s;
        }
        private string GetOSArchitecture()
        {
            string result = "";
            if (Environment.Is64BitOperatingSystem)
            {
                result = "64-bit";
            }
            else
            {
                result = "32-bit";
            }
            return result;
        }
        private string GetOSInfo(string property)
        {
            string result = "";
            if (property == null || property.Length == 0)
            {
                return null;
            }
            try
            {
                var queryStr = string.Format($"SELECT {property} FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(queryStr);
                ManagementObjectCollection collection = searcher.Get();
                result = (from x in collection.Cast<ManagementObject>()
                          select x.GetPropertyValue(property)).FirstOrDefault().ToString();
                collection.Dispose();
                searcher.Dispose();
            }
            catch (Exception e)
            {
                SdkLogger.Instance.Error($"{e}");
            }
            return result;
        }
    }
}
