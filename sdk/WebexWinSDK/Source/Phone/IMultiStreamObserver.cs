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
    /// <summary>
    /// The interface of multi stream. Client must implement this interface to enable the multi-stream feature.
    /// </summary>
    /// <remarks>Since: 2.0.0</remarks>
    public interface IMultiStreamObserver
    {
        /// <summary>
        /// Callback of SDK when there is a new available auxiliary stream.
        /// Client should give SDK a view handle for rendering, and the AuxStreamOpenedEvent would be triggered indicating whether the stream is successfully opened.
        /// If the client don't want to open stream, return IntPtr.Zero.
        /// </summary>
        /// <returns>The handle of the view.</returns>
        /// <remarks>Since: 2.0.0</remarks>
        IntPtr OnAuxStreamAvailable();

        /// <summary>
        /// Callback of SDK when there is a auxiliary stream unavailable.
        /// Client should give SDK a view handle which will be closed or if the given view handle is IntPtr.Zero, SDK will automatically close the last opened stream.
        /// </summary>
        /// <returns>The handle of the view</returns>
        /// <remarks>Since: 2.0.0</remarks>
        IntPtr OnAuxStreamUnAvailable();

        /// <summary>
        /// Callback of auxiliary stream related events.
        /// </summary>
        /// <param name="auxStreamEvent">the auxiliary stream related event</param>
        /// <remarks>Since: 2.0.0</remarks>
        void OnAuxStreamEvent(AuxStreamEvent auxStreamEvent);
    }
}
