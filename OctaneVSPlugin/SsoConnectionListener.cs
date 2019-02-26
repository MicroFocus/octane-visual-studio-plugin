/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/


using MicroFocus.Adm.Octane.Api.Core.Connector.Authentication;
using MicroFocus.Adm.Octane.VisualStudio.View;
using System;
using System.Windows;

namespace MicroFocus.Adm.Octane.VisualStudio
{
	class SsoConnectionListener : ConnectionListener
	{

		private BrowserDialog browserDialog;

		public SsoConnectionListener()
		{
            
        }

		public void OpenBrowser(string url)
		{
            // Something keeps settings this back to IE9, force reset it to latest every time the window opens
            EmbeddedBrowserUtil.SetBrowserEmulationVersionToLatestIE();
            Application.Current.Dispatcher.Invoke(new Action(() =>
			{
                browserDialog = new BrowserDialog();
                browserDialog.Show(url);
			}));

		}

        public void UpdateTimeout(int timeout)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                browserDialog.UpdateTimeoutMessage(timeout);
            }));
        }

		public void CloseBrowser()
		{
			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				browserDialog.Close();
			}));
		}

        public bool IsOpen()
        {
            return browserDialog.IsOpen;
        }
	}
}