﻿// Copyright (c) 2014-2015 Wolfgang Borgsmüller
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// 1. Redistributions of source code must retain the above copyright 
//    notice, this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright 
//    notice, this list of conditions and the following disclaimer in the 
//    documentation and/or other materials provided with the distribution.
// 
// 3. Neither the name of the copyright holder nor the names of its 
//    contributors may be used to endorse or promote products derived 
//    from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE 
// COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS 
// OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND 
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
// USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chromium.Remote;

namespace Chromium.WebBrowser {

    /// <summary>
    /// The type of a javascript property.
    /// </summary>
    public enum JSPropertyType {
        Function,
        Object
    }

    /// <summary>
    /// Represents a javascript property in the render process to be added to 
    /// a browser frame's global object or to a JSObject.
    /// </summary>
    public abstract class JSProperty {

        /// <summary>
        /// The type of this property.
        /// </summary>
        public JSPropertyType PropertyType { get; private set; }

        /// <summary>
        /// The name of this property.
        /// May be null if this property is still unbound.
        /// </summary>
        public string Name { get; private set; }

        private ChromiumWebBrowser m_browser;
        private JSObject m_parent;

        internal JSProperty(JSPropertyType type) {
            PropertyType = type;
        }

        /// <summary>
        /// The browser this JSProperty or the parent JSObject belongs to.
        /// </summary>
        public ChromiumWebBrowser Browser {
            get {
                if(m_parent != null)
                    return m_parent.GetBrowserFromParent(this);
                return m_browser;
            }
        }

        /// <summary>
        /// The parent JSObject of this property, or null if this property
        /// was not added to a JSObject.
        /// </summary>
        public JSObject Parent {
            get {
                return m_parent;
            }
        }

        internal abstract CfrV8Value GetV8Value(CfrRuntime remoteRuntime);

        private ChromiumWebBrowser GetBrowserFromParent(JSProperty requestor) {

            if(m_parent == null)
                return null;

            if(m_parent.m_browser != null)
                return m_parent.m_browser;

            if(Object.ReferenceEquals(m_parent, requestor))
                return null;

            return m_parent.GetBrowserFromParent(requestor);
        }

        internal void SetParent(string propertyName, JSObject parent) {
            CheckUnboundState();
            Name = propertyName;
            m_parent = parent;
        }

        internal void SetBrowser(string propertyName, ChromiumWebBrowser browser) {
            CheckUnboundState();
            Name = propertyName;
            m_browser = browser;
        }

        private void CheckUnboundState() {
            if(m_parent != null) {
                throw new CfxException("This property already belongs to an JSObject.");
            }
            if(m_browser != null) {
                throw new CfxException("This property already belongs to a browser frame's global object.");
            }
        }
    }
}