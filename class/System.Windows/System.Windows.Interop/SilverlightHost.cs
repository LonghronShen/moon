//
// SilverlightHost.cs
//
// Contact:
//   Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright 2008, 2009 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Mono;

namespace System.Windows.Interop {
	public class SilverlightHost {
		Content content;
		Settings settings;
		Uri source_uri;
		private Dictionary<string,string> init_params;
		private string navigation_state;

		public SilverlightHost ()
		{
		}

		public bool IsVersionSupported (string versionStr)
		{
			// no null check so we throw an NRE, just like Silverlight, for a null versionStr
			if (versionStr.Length == 0)
				return false;
			return NativeMethods.surface_is_version_supported (versionStr);
		}

		public Color Background {
			get {
				IntPtr clr = NativeMethods.surface_get_background_color (Deployment.Current.Surface.Native);
				
				if (clr == IntPtr.Zero)
					return new Color ();
				
				unsafe {
					return ((UnmanagedColor *) clr)->ToColor ();
				}
			}
		}

		public Content Content {
			get { return content ?? (content = new Content ()); }
		}

		public bool IsLoaded {
			get { 
				return NativeMethods.surface_is_loaded (Deployment.Current.Surface.Native);
			}
		}

		public Settings Settings {
			get { return settings ?? (settings = new Settings ()); }
		}

		public Uri Source {
			get {
				if (source_uri == null) {
					if (PluginHost.Handle == IntPtr.Zero) {
						string source = NativeMethods.surface_get_source_location (Deployment.Current.Surface.Native);

						source_uri = new Uri (source, UriKind.RelativeOrAbsolute);
					}
					else {
						// note: this must return the original URI (i.e. before any redirection)
						string source = NativeMethods.plugin_instance_get_source_original (PluginHost.Handle);
						source_uri = new Uri (source, UriKind.RelativeOrAbsolute);
						// the source is often relative (but can be absolute for cross-domain applications)
						if (!source_uri.IsAbsoluteUri) {
							string location = NativeMethods.plugin_instance_get_source_location_original (PluginHost.Handle);
							source_uri = new Uri (new Uri (location), source_uri);
						}
					}
				}
				return source_uri;
			}
		}

		public IDictionary<string,string> InitParams {
			get {
				if (init_params != null)
					return init_params;

				init_params = new Dictionary<string,string> ();

				if (PluginHost.Handle != IntPtr.Zero) {
					char [] param_separator = new char [] { ',' };
					
					string param_string = NativeMethods.plugin_instance_get_init_params (PluginHost.Handle);
					// Console.WriteLine ("params = {0}", param_string);
					if (!String.IsNullOrEmpty (param_string)) {
						foreach (string val in param_string.Split (param_separator)) {
							int split = val.IndexOf ('=');
							if (split >= 0) {
								string k = val.Substring (0, split).Trim ();
								string v = val.Substring (split + 1).Trim ();
								if (k.Length > 0)
									init_params [k] = v;
							} else {
								string s = val.Trim ();
								if (!String.IsNullOrEmpty (s))
									init_params [s] = String.Empty;
							}
						}
					}
				}

				return init_params;
			}
		}

		[MonoTODO ("incomplete - would be easier from S.W.Browser.dll")]
		public string NavigationState {
			get {
				if (navigation_state == null) {
					navigation_state = String.Empty;
#if false
					// look for an iframe named _sl_historyFrame in the web page and throw if not found
					HtmlDocument doc = System.Windows.Browser.HtmlPage.Document;
					HtmlElement iframe = doc.GetElementById ("_sl_historyFrame");
					if ((iframe == null) || (iframe.TagName != "iframe"))
						throw new InvalidOperationException ("missing <iframe id=\"_sl_historyFrame\">");

					// return the fragment of the web page URL as the initial state, if none then String.Empty
					string state = doc.DocumentUri.Fragment;
					if (!String.IsNullOrEmpty (state))
						navigation_state = state.Substring (1); // remove '#'
#endif
				}
				return navigation_state;
			}
			set {
				string current = navigation_state;
				if (current != value) {
					navigation_state = value;
					var changed = NavigationStateChanged;
					if (changed != null)
						changed (this, new NavigationStateChangedEventArgs (current, value));
				}
			}
		}

		public event EventHandler<NavigationStateChangedEventArgs> NavigationStateChanged;
	}
}
