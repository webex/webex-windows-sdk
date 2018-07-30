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

        private UserAgent()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Version FROM Win32_OperatingSystem");
            ManagementObjectCollection collection = searcher.Get();
            var OSBuildNumber = (from x in collection.Cast<ManagementObject>()
                                 select x.GetPropertyValue("Version")).FirstOrDefault();

            OSVersion = OSBuildNumber != null ? OSBuildNumber.ToString() : "Unknown";
            OSVersion = "Microsoft Windows " + OSVersion;

            var OSLanguage = (from x in new ManagementObjectSearcher("SELECT OSLanguage FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                              select x.GetPropertyValue("OSLanguage")).FirstOrDefault();
            this.OSLanguage = GetOSLanguageName(int.Parse(OSLanguage.ToString()));

            collection.Dispose();
            searcher.Dispose();
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
                return string.Format($"webex_win_sdk {Webex.Version}/({OSVersion})");
            } 
        }

        private string GetOSLanguageName(int language)
        {
            switch (language)
            {
                case 1078: return "Afrikaans";
                case 1052: return "Albanian";
                case 1025: return "Arabic (Saudi Arabia)";
                case 2049: return "Arabic (Iraq)";
                case 3073: return "Arabic (Eqypt)";
                case 4097: return "Arabic (Libya)";
                case 5121: return "Arabic (Algeria)";
                case 6145: return "Arabic (Morocco)";
                case 7169: return "Arabic (Tunisia)";
                case 8193: return "Arabic (Oman)";
                case 9217: return "Arabic (Yemen)";
                case 10241: return "Arabic (Syria)";
                case 11265: return "Arabic (Jordon)";
                case 12289: return "Arabic (Lebanon)";
                case 13313: return "Arabic (Kuwait)";
                case 14337: return "Arabic (UAE)";
                case 15361: return "Arabic (Bahrain)";
                case 16385: return "Arabic (Qatar)";
                case 1067: return "Armenian";
                case 1068: return "Azeri (Latin)";
                case 2092: return "Azeri (Cyrillic)";
                case 1069: return "Basque";
                case 1059: return "Belarusian";
                case 1093: return "Bengali (India)";
                case 5146: return "Bosnian (Latin)";
                case 1026: return "Bulgarian";
                case 1027: return "Catalan";
                case 1028: return "Chinese (Taiwan)";
                case 2052: return "Chinese (PRC)";
                case 3076: return "Chinese (Hong Kong)";
                case 4100: return "Chinese (Singapore)";
                case 5124: return "Chinese (Macau)";
                case 1050: return "Croatian";
                case 4122: return "Croatian (Bosnia Herzegovina)";
                case 1029: return "Czech";
                case 1030: return "Danish";
                case 1125: return "Divehi";
                case 1043: return "Dutch (Standard)";
                case 2067: return "Dutch (Belgian)";
                case 1033: return "English (United States)";
                case 2057: return "English (United Kingdom)";
                case 3081: return "English (Australian)";
                case 4105: return "English (Canadian)";
                case 5129: return "English (New Zealand)";
                case 6153: return "English (Ireland)";
                case 7177: return "English (South Africa)";
                case 8201: return "English (Jamaica)";
                case 9225: return "English (Caribbean)";
                case 10249: return "English (Belize)";
                case 11273: return "English (Trinidad)";
                case 12297: return "English (Zimbabwe)";
                case 13321: return "English (Philippines)";
                case 1061: return "Estonian";
                case 1080: return "Faeroese";
                case 1065: return "Farsi";
                case 1035: return "Finnish";
                case 1036: return "French (Standard)";
                case 2060: return "French (Belgian)";
                case 3084: return "French (Canadian)";
                case 4108: return "French (Swiss)";
                case 5132: return "French (Luxembourg)";
                case 6156: return "French (Monaco)";
                case 1079: return "Georgian";
                case 1110: return "Galician";
                case 1031: return "German (Standard)";
                case 2055: return "German (Swiss)";
                case 3079: return "German (Austrian)";
                case 4103: return "German (Luxembourg)";
                case 5127: return "German (Liechtenstein)";
                case 1032: return "Greek";
                case 1095: return "Gujarati";
                case 1037: return "Hebrew";
                case 1081: return "Hindi";
                case 1038: return "Hungarian";
                case 1039: return "Icelandic";
                case 1057: return "Indonesian";
                case 1040: return "Italian (Standard)";
                case 2064: return "Italian (Swiss)";
                case 1041: return "Japanese";
                case 1099: return "Kannada";
                case 1087: return "Kazakh";
                case 1111: return "Konkani";
                case 1042: return "Korean";
                case 1088: return "Kyrgyz";
                case 1062: return "Latvian";
                case 1063: return "Lithuanian";
                case 1071: return "Macedonian";
                case 1086: return "Malay (Malaysia)";
                case 2110: return "Malay (Brunei Darussalam)";
                case 1100: return "Malayalam";
                case 1082: return "Maltese";
                case 1153: return "Maori";
                case 1102: return "Marathi";
                case 1104: return "Mongolian";
                case 1044: return "Norwegian (Bokmal)";
                case 2068: return "Norwegian (Nynorsk)";
                case 1045: return "Polish";
                case 1046: return "Portuguese (Brazilian)";
                case 2070: return "Portuguese (Standard)";
                case 1094: return "Punjabi";
                case 1131: return "Quechua (Bolivia)";
                case 2155: return "Quechua (Ecuador)";
                case 3179: return "Quechua (Peru)";
                case 1048: return "Romanian";
                case 1049: return "Russian";
                case 9275: return "Sami (Inari)";
                case 4155: return "Sami (Lule Norway)";
                case 5179: return "Sami (Lule Sweden)";
                case 3131: return "Sami (Northern Finland)";
                case 1083: return "Sami (Northern Norway)";
                case 2107: return "Sami (Northern Sweden)";
                case 8251: return "Sami (Skolt)";
                case 6203: return "Sami (Southern Norway)";
                case 7227: return "Sami (Southern Sweden)";
                case 1103: return "Sanskrit";
                case 2074: return "Serbian (Latin)";
                case 6170: return "Serbian (Latin Bosnia Herzegovina)";
                case 3098: return "Serbian (Cyrillic)";
                case 7194: return "Serbian (Cyrillic Bosnia Herzegovina)";
                case 1051: return "Slovak";
                case 1060: return "Slovenian";
                case 1034: return "Spanish (Traditional Sort)";
                case 2058: return "Spanish (Mexican)";
                case 3082: return "Spanish (Modern Sort)";
                case 4106: return "Spanish (Gautemala)";
                case 5130: return "Spanish (Costa Rica)";
                case 6154: return "Spanish (Panama)";
                case 7178: return "Spanish (Dominican Republic)";
                case 8202: return "Spanish (Venezuela)";
                case 9226: return "Spanish (Colombia)";
                case 10250: return "Spanish (Peru)";
                case 11274: return "Spanish (Argentina)";
                case 12298: return "Spanish (Ecuador)";
                case 13322: return "Spanish (Chile)";
                case 14346: return "Spanish (Uruguay)";
                case 15370: return "Spanish (Paraguay)";
                case 16394: return "Spanish (Bolivia)";
                case 17418: return "Spanish (El Salvador)";
                case 18442: return "Spanish (Honduras)";
                case 19466: return "Spanish (Nicaragua)";
                case 20490: return "Spanish (Puerto Rico)";
                case 1089: return "Swahili";
                case 1053: return "Swedish";
                case 2077: return "Swedish (Finland)";
                case 1114: return "Syriac";
                case 1097: return "Tamil";
                case 1092: return "Tatar";
                case 1098: return "Telugu";
                case 1054: return "Thai";
                case 1074: return "Tswana";
                case 1058: return "Ukrainian";
                case 1055: return "Turkish";
                case 1056: return "Urdu";
                case 1091: return "Uzbek (Latin)";
                case 2115: return "Uzbek (Cyrillic)";
                case 1066: return "Vietnamese";
                case 1106: return "Welsh";
                case 1076: return "Xhosa";
                case 1077: return "Zulu";
                default: return "Unknown";
            }
        }
    }
}
