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
    internal class StringExtention
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
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            
            return System.Text.Encoding.UTF8.GetString(converted);
        }

        public static string Base64UrlEncode(string input)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return System.Convert.ToBase64String(plainTextBytes).Replace("=", "").Replace('+', '-').Replace('/', '_');
        }
        public enum HydraIdType
        {
            Error,
            People,
            Space,
            Message,
            Unknow,
        }
        public static string EncodeHydraId(HydraIdType type, string address)
        {
            string peopleUrl = "ciscospark://us/PEOPLE/";
            string spaceUrl = "ciscospark://us/ROOM/";
            string messageUrl = "ciscospark://us/MESSAGE/";

            string result=null;

            switch (type)
            {
                case HydraIdType.Space:
                    result = Base64UrlEncode(spaceUrl + address);
                    break;
                case HydraIdType.People:
                    result = Base64UrlEncode(peopleUrl + address);
                    break;
                case HydraIdType.Message:
                    result = Base64UrlEncode(messageUrl + address);
                    break;
                default:
                    break;
            }
            return result;
                
        }
        public static HydraIdType ParseHydraId(string address, ref string outputAddress)
        {
            string peopleUrl = "ciscospark://us/PEOPLE/";
            string spaceUrl = "ciscospark://us/ROOM/";
            string messageUrl = "ciscospark://us/MESSAGE/";

            outputAddress = null;
            HydraIdType result;

            try
            {
                var decodedStr = StringExtention.Base64UrlDecode(address);
                if (decodedStr.StartsWith(peopleUrl))
                {
                    outputAddress = decodedStr.Substring(peopleUrl.Length);
                    result = HydraIdType.People;
                }
                else if (decodedStr.StartsWith(spaceUrl))
                {
                    outputAddress = decodedStr.Substring(spaceUrl.Length);
                    result = HydraIdType.Space;
                }
                else if (decodedStr.StartsWith(messageUrl))
                {
                    outputAddress = decodedStr.Substring(messageUrl.Length);
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
}
