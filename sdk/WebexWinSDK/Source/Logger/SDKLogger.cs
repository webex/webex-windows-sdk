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

        public void Debug(String format, params object[] args)
        {
            string str = string.Format(format, args);
            Log(LogLevel.Debug, str);
        }

        public void Info(String format, params object[] args)
        {
            string str = string.Format(format, args);
            Log(LogLevel.Info, str);
        }

        public void Warn(String format, params object[] args)
        {
            string str = string.Format(format, args);
            Log(LogLevel.Warn, str);
        }
        public void Error(String format, params object[] args)
        {
            string str = string.Format(format, args);
            Log(LogLevel.Error, str);
        }

        public void Output(LogLevel logLevel, String format, params object[] args)
        {
            string str = string.Format(format, args);
            Log(logLevel, str);
        }

        private void Log(LogLevel logLevel, string str)
        {
            StackTrace st = new StackTrace(true);

            StackFrame sf = st.GetFrame(2);

            string fullstr = string.Format("{0,-10}", "[WebexSDK]");
            fullstr += string.Format(" {0}:{1} {2}::{3} {4}", System.IO.Path.GetFileName(sf.GetFileName()), sf.GetFileLineNumber(), sf.GetMethod().ReflectedType.Name, sf.GetMethod().Name, str);
            
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
