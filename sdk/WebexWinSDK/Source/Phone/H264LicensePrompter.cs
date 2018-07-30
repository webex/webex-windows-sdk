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
    internal class H264LicensePrompter
    {
        public string License
        {
            get
            {
                return @"To enable video calls, activate a free video license (H.264 AVC) from Cisco. By selecting 'Activate', you accept the Cisco End User License Agreement and Notices.";
            } 
        }

        public string LicenseURL
        {
            get
            {
                return @"http://www.openh264.org/BINARY_LICENSE.txt";
            }
        }

        public bool IsVideoLicenseActivated
        {
            get
            {
                string strVideoLicenseActivated = null;
                SCFCore.Instance.m_core.getValue("isVideoLicenseActivatedKey", ref strVideoLicenseActivated);
                return String.Equals(strVideoLicenseActivated, "TRUE", StringComparison.CurrentCultureIgnoreCase);
            }
            set
            {
                SDKLogger.Instance.Info($"set isVideoLicenseActivated to {value}");
                SCFCore.Instance.m_core.setValue("isVideoLicenseActivatedKey", value.ToString());
            }
        }

        public bool IsVideoLicenseActivationDisabled
        {
            get
            {
                string strVideoLicenseActivatedDisabled = null;
                SCFCore.Instance.m_core.getValue("isVideoLicenseActivationDisabledKey", ref strVideoLicenseActivatedDisabled);
                return String.Equals(strVideoLicenseActivatedDisabled, "TRUE", StringComparison.CurrentCultureIgnoreCase);
            }
            set
            {
                SDKLogger.Instance.Info($"set isVideoLicenseActivationDisabled to {value}");
                SCFCore.Instance.m_core.setValue("isVideoLicenseActivationDisabledKey", value.ToString());
            }
        }

        public bool Check()
        {
            if (IsVideoLicenseActivated || IsVideoLicenseActivationDisabled)
            {
                return true;
            }
            return false;
        }

    }
}
