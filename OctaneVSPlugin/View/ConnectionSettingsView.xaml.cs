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

using System;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// Interaction logic for ConnectionSettingsView.xaml
    /// </summary>
    public partial class ConnectionSettingsView : UserControl
    {
        private DispatcherTimer _typingTimer;
        public ConnectionSettingsView()
        {
            InitializeComponent();
        }

        internal ConnectionSettings optionsPage;

        public void Initialize()
        {
            this.DataContext = optionsPage;
            try
            {
                passwordTextBox.Password = optionsPage.Password;
            }
            catch (Exception)
            {
                passwordTextBox.Password = "";
            }
        }

        private async void TestConnection(object sender, RoutedEventArgs e)
        {
            InfoLabel.Text = "";
            SetInfoLabelText(await optionsPage.TestConnection());
        }

        private void ClearSettings(object sender, RoutedEventArgs e)
        {
            serverUrlTextBox.Text = "";
            sharedSpaceTextBox.Text = "";
            workspaceTextBox.Text = "";
            usernameTextBox.Text = "";
            passwordTextBox.Password = "";
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (optionsPage != null)
            {
                optionsPage.Password = ((PasswordBox)sender).Password;
            }
        }

        public void SetInfoLabelText(string text)
        {
            InfoLabel.Text = text;
            if (text.Equals(ConnectionSettings.ConnectionSuccessful))
            {
                InfoLabel.Foreground = Brushes.Green;
            }
            else
            {
                InfoLabel.Foreground = Brushes.Red;
            }
        }

        public void SetSharedWorkspaceDetails(long sharedSpace, long workspace)
        {
            sharedSpaceTextBox.Text = sharedSpace.ToString();
            workspaceTextBox.Text = workspace.ToString();
        }

        private void serverUrlTextBox_TextChanged(object sender, EventArgs e)
        {

            if (_typingTimer == null)
            {
                _typingTimer = new DispatcherTimer();
                _typingTimer.Interval = TimeSpan.FromMilliseconds(300);
                _typingTimer.Tick += new EventHandler(this.handleTypingTimerTimeout);
            }

            _typingTimer.Stop(); // Resets the timer
            _typingTimer.Tag = (sender as TextBox).Text; // This should be done with EventArgs
            _typingTimer.Start();
        }

        private void handleTypingTimerTimeout(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;

            if (timer == null)
            {
                return;
            }

            var authenticationUrl = timer.Tag.ToString();

            Uri siteUrl;
            
            string baseUrl = authenticationUrl;

            // reset the error message if any
            SetInfoLabelText("");

            // if no url given no reason to perform analisys
            if("".Equals(authenticationUrl))
            {
                return;
            }
            
            try
            {
                siteUrl = new Uri(authenticationUrl);

                // get rid of what we don't need
                var builder = new UriBuilder(siteUrl.Scheme, siteUrl.Host, siteUrl.Port);

                baseUrl = builder.Uri.ToString();

                string siteUrlLocalPath = siteUrl.LocalPath;
                if (siteUrlLocalPath.EndsWith("/ui/")) 
                {
                    baseUrl += siteUrlLocalPath.Substring(1, siteUrlLocalPath.Length - 4);
                }
                else
                {
                    baseUrl += siteUrlLocalPath.Substring(1, siteUrlLocalPath.Length - 1);
                }
                
                // trim the "/" from the end of the url
                baseUrl = baseUrl.TrimEnd('/');

                serverUrlTextBox.Text = baseUrl;

                if (!"http".Equals(siteUrl.Scheme) && !"https".Equals(siteUrl.Scheme))
                {
                    throw new Exception();
                }

            }
            catch (Exception)
            {
                SetInfoLabelText("Given server URL is not valid.");
            }

            // try to extract sharedspace and workspace id from the url
            try
            {
                siteUrl = new Uri(authenticationUrl);
                long sharedspaceId = -1;
                long workspaceId = -1;
                string queryParams = HttpUtility.ParseQueryString(siteUrl.Query).Get("p");
                if (queryParams != null)
                {
                    string[] split = queryParams.Split('/');
                    sharedspaceId = long.Parse(split[0].Trim());
                    workspaceId = long.Parse(split[1].Trim());
                }

                if (workspaceId >= 0 && sharedspaceId >= 0)
                {
                        SetSharedWorkspaceDetails(sharedspaceId, workspaceId);
                }
            }
            catch (Exception)
            {
                // do nothing just dont break
            }

            timer.Stop();
        }
    }
}
