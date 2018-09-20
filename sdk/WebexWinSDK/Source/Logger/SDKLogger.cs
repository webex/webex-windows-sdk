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
using System.Diagnostics;
using SparkNet;

namespace WebexSDK
{
    internal sealed class SdkLogger
    {
        public ILogger Logger { get; set; }
        private LogLevel console = LogLevel.Debug;


        private static volatile SdkLogger instance = null;
        private static readonly object lockHelper = new object();

        public static SdkLogger Instance
        {
            get
            {
                if (null == instance)
                {
                    lock (lockHelper)
                    {
                        if (null == instance)
                        {
                            instance = new SdkLogger();
                        }
                    }

                }
                return instance;
            }
        }

        public LogLevel Console
        {
            get { return console; }
            set
            {
                console = value;
                
                //set core log level
                SCFCore.Instance.m_core.setLogLevel((SCFLogLevel)value);
            }
        }

        public void Debug(String str,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            Log(LogLevel.Debug, str, memberName, sourceFilePath, sourceLineNumber);
        }

        public void Info(String str,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            Log(LogLevel.Info, str, memberName, sourceFilePath, sourceLineNumber);
        }

        public void Warn(String str,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            Log(LogLevel.Warn, str, memberName, sourceFilePath, sourceLineNumber);
        }

        public void Error(String str,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            Log(LogLevel.Error, str, memberName, sourceFilePath, sourceLineNumber);
        }

        public void Output(LogLevel logLevel, String str,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            Log(logLevel, str, memberName, sourceFilePath, sourceLineNumber);
        }

        private void Log(LogLevel logLevel, string str, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            string fullstr = string.Format("{0,-10}", "[WebexSDK]");
            fullstr += string.Format(" {0}:{1} {2}: {3}", System.IO.Path.GetFileName(sourceFilePath), sourceLineNumber, memberName, str);
            if (logLevel >= Console)
            {
                SCFCore.Instance.m_core.outputLog((SCFLogLevel)logLevel, fullstr);

                if (this.Logger != null)
                {
                    string loggerOutput = string.Format("{0,-19}", $"{DateTime.UtcNow}");
                    loggerOutput += string.Format("{0,-5}", $"{logLevel.ToString()}");
                    loggerOutput += string.Format("{0,-5}", $"[{Thread.CurrentThread.ManagedThreadId}]");
                    loggerOutput += string.Format($"{fullstr}");
                    this.Logger.Log(loggerOutput);
                }
            }
        }
    }
}
