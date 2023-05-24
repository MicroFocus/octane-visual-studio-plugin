/*!
* Copyright 2017 – 2023 Open Text.
*
* The only warranties for products and services of Open Text and
* its affiliates and licensors (“Open Text”) are as may be set
* forth in the express warranty statements accompanying such products
* and services. Nothing herein should be construed as constituting an
* additional warranty. Open Text shall not be liable for technical or
* editorial errors or omissions contained herein. The information
* contained herein is subject to change without notice.
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
using System.Windows.Threading;

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
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
			{
                browserDialog = new BrowserDialog();
                browserDialog.Show(url);
			}));

		}

        public void UpdateTimeout(int timeout)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                browserDialog.UpdateTimeoutMessage(timeout);
            }));
        }

		public void CloseBrowser()
		{
			Application.Current.Dispatcher.BeginInvoke(new Action(() =>
			{
				browserDialog.Close();
			}));
		}

        public bool IsOpen()
        {
            if (browserDialog == null)
            {
                return true;
            }
            else
            {
                return browserDialog.IsOpen;
            }
        }
	}
}