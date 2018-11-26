﻿/*!
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

using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.View;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Options page represent the settings the user can set for ALM Octane plugin.
    /// This class is presented to the user as a page in Visual Studio options dialog.
    /// </summary>
    internal class OptionsPage : DialogPage
    {
        const string category = "Server Settings";

        private string url = string.Empty;
        private int ssid = 1001;
        private int wsid = 1002;
        private string user = string.Empty;
        private string password = string.Empty;

        /// <inheritdoc/>
        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            // reset and thus require a new octane service obj
            OctaneServices.Reset();
            
            // close all opened details windows so that we don't have details windows
            // for entities from different workspaces
            PluginWindowManager.CloseAllDetailsWindows();
        
            // disable the active item toolbar because we don't know yet
            // whether we can connect with the new Octane credentials
            MainWindowCommand.Instance.DisableActiveItemToolbar();

            // After settings are applied we notify the main ViewModel to allow it to refresh.
            if (OctaneMyItemsViewModel.Instance != null)
            {
                OctaneMyItemsViewModel.Instance.LoadMyItemsAsync();
                EntityTypeRegistry.Init();
            }
        }

        [Category(category)]
        [DisplayName("1. Server URL")]
        [Description("Format: http://servername:8081 (do not include additional info)")]
        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                OctaneConfiguration.Url = url;
            }
        }

        [Category(category)]
        [DisplayName("2. Shared Space ID")]
        public int SsId
        {
            get { return ssid; }
            set
            {
                ssid = value;
                OctaneConfiguration.SharedSpaceId = ssid;
            }
        }

        [Category(category)]
        [DisplayName("3. Workspace ID")]
        public int WsId
        {
            get { return wsid; }
            set
            {
                wsid = value;
                OctaneConfiguration.WorkSpaceId = wsid;
            }
        }


        [Category(category)]
        [DisplayName("4. User")]
        public string User
        {
            get { return user; }
            set
            {
                user = value;
                OctaneConfiguration.Username = user;
            }
        }

        [Category(category)]
        [DisplayName("5. Password")]
        [PasswordPropertyText(true)]
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OctaneConfiguration.Password = password;
            }
        }

        public override void LoadSettingsFromStorage()
        {
            // After loading the settings from storage, we'll get the encrypted password.
            // We then decrypt the password to allow the extension to use it.
            base.LoadSettingsFromStorage();
            DecryptPassword();
        }

        public override void SaveSettingsToStorage()
        {
            // Before saving the settings we encrypt the password using Windows Data Protection API.
            // Then we save the settings and then we decrypt the password to allow the extension to use it.
            EncryptPassword();
            base.SaveSettingsToStorage();
            DecryptPassword();
        }

        private void EncryptPassword()
        {
            if (Password != null)
            {
                Password = DataProtector.Protect(Password);
            }
        }

        private void DecryptPassword()
        {
            if (Password != null)
            {
                Password = DataProtector.Unprotect(Password);
            }
        }

    }
}
