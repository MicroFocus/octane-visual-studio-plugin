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

using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows;
using System.Windows.Controls;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{

    /// <summary>
    /// Interaction logic for BrowserDialog.xaml
    /// </summary>
    public partial class BrowserDialog : DialogWindow
    {
        private static string _browserTitle = "Login to ValueEdge";

        public bool IsOpen { get; set; }

        public string Url { get; set; }

        public BrowserDialog()
        {
            InitializeComponent();            
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private const int INTERNET_OPTION_SUPPRESS_BEHAVIOR = 81;
        private const int INTERNET_SUPPRESS_COOKIE_PERSIST = 3;

        public static void SuppressCookiePersistence()
        {
            var lpBuffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)));
            Marshal.StructureToPtr(INTERNET_SUPPRESS_COOKIE_PERSIST, lpBuffer, true);

            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SUPPRESS_BEHAVIOR, lpBuffer, sizeof(int));

            Marshal.FreeCoTaskMem(lpBuffer);
        }

        private void hyperlink_Click(object sender, RoutedEventArgs e)
        {
            // Open the URL in the user's default browser.
            Process.Start(Url);
        }
        
        public void Show(string Url)
        {
            this.Url = Url;
            this.IsOpen = true;
            SuppressCookiePersistence();
            browser.Navigate(new Uri(Url));
            Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.IsOpen = false;
        }

        public void UpdateTimeoutMessage(int timeout)
        {
            Title = _browserTitle + " (waiting for session, timeout in " + timeout + " )";
        }

    }
}