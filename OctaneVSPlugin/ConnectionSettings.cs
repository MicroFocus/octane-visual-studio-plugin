/*!
* © Copyright 2017-2022 Micro Focus or one of its affiliates.
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

using MicroFocus.Adm.Octane.Api.Core.Connector;
using MicroFocus.Adm.Octane.Api.Core.Connector.Authentication;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.View;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static System.Threading.Tasks.Task;

namespace MicroFocus.Adm.Octane.VisualStudio
{
	/// <summary>
	/// Options page represent the settings the user can set for ALM Octane plugin.
	/// This class is presented to the user as a page in Visual Studio options dialog.
	/// </summary>
	[Guid("1D9ECCF3-5D2F-4112-9B25-264596873DC9")]
	internal class ConnectionSettings : UIElementDialogPage
	{

		public const string ConnectionSuccessful = "Connection successful!";
		private string url = string.Empty;
		private long ssid = 0;
		private long wsid = 0;
		private string user = string.Empty;
		private string password = string.Empty;
		private bool credentialLogin = true;
		private bool ssologin = false;
		private ConnectionSettingsView page;

		private UserPassConnectionInfo oldUserPassConnectionInfo;
		private long oldSsid = 1001;
		private long oldWsid = 1002;
		private string oldUrl = "";


		protected override void OnApply(PageApplyEventArgs e)
		{

			page.SetInfoLabelText("");

			// show welcome view if user clears the URL
			if ("".Equals(OctaneConfiguration.Url))
            {
				if (OctaneMyItemsViewModel.Instance != null)
				{
					OctaneMyItemsViewModel.Instance.Mode = MainWindowMode.FirstTime;
				}
                return;
            }

			var result = Run(async () => { return await TestConnection(); }).Result;
			if (!result.Equals(ConnectionSuccessful))
			{
				e.ApplyBehavior = ApplyKind.CancelNoNavigate;
			}
			else
			{
				setOldCredentials();

				// reset and thus require a new octane service obj
				Run(async () => { return await OctaneServices.Reset(); }).Wait();

				// create a new service object
				OctaneServices.Create(OctaneConfiguration.Url,
						  OctaneConfiguration.SharedSpaceId,
						  OctaneConfiguration.WorkSpaceId);

                
                // close all opened details windows so that we don't have details windows
                // for entities from different workspaces
                PluginWindowManager.CloseAllDetailsWindows();

				// disable the active item toolbar because we don't know yet
				// whether we can connect with the new Octane credentials
				MainWindowCommand.Instance.DisableActiveItemToolbar();

				openToolWindow();

				// After settings are applied we notify the main ViewModel to allow it to refresh.
				if (OctaneMyItemsViewModel.Instance != null)
				{
					InitialisePluginComponents();
				}

				if (ssologin)
				{
					clearUserPassTextBoxes();
				}

				base.OnApply(e);
			}

			page.SetInfoLabelText(result);
		}

        protected override void OnClosed(EventArgs e)
        {
			
			if (credentialLogin)
            {
				if ("".Equals(OctaneConfiguration.Url))
				{
					if (OctaneMyItemsViewModel.Instance != null)
					{
						OctaneMyItemsViewModel.Instance.Mode = MainWindowMode.FirstTime;
					}
				}
				else
				{
					Run(async () => { return await OctaneServices.Reset(); }).Wait();

					OctaneConfiguration.Username = oldUserPassConnectionInfo.user;
					OctaneConfiguration.Password = oldUserPassConnectionInfo.password;
					ssid = oldSsid;
					wsid = oldWsid;
					OctaneServices.Create(oldUrl, oldSsid, oldWsid);

					try
					{
						Run(async () => { await OctaneServices.GetInstance().Connect(); }).Wait();
					}
					catch
					{
						clearUserPassTextBoxes();
					}
					finally
					{
						page.SetInfoLabelText("");
					}
					
					base.OnClosed(e);
				}
			}
		}

        private void openToolWindow()
        {
			IVsUIShell vsUIShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
			Guid guid = typeof(MainWindow).GUID;
			IVsWindowFrame windowFrame;
			int result = vsUIShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref guid, out windowFrame);   // Find MyToolWindow

			if (result != VSConstants.S_OK)
				result = vsUIShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref guid, out windowFrame); // Crate MyToolWindow if not found

			if (result == VSConstants.S_OK)                                                                           // Show MyToolWindow
				ErrorHandler.ThrowOnFailure(windowFrame.Show());
		}

        public async void InitialisePluginComponents()
        {
            await OctaneServices.GetInstance().Connect();
            await OctaneMyItemsViewModel.Instance.LoadMyItemsAsync();
            await EntityTypeRegistry.Init();
        } 

		public async Task<string> TestConnection()
		{
			try
			{
				AuthenticationStrategy authenticationStrategy = null;
				if (credentialLogin)
				{
					authenticationStrategy = new LwssoAuthenticationStrategy(new UserPassConnectionInfo(OctaneConfiguration.Username,
																										OctaneConfiguration.Password));
				}
				else if (ssologin)
				{
					authenticationStrategy = new SsoAuthenticationStrategy();
				}
				
				await authenticationStrategy.TestConnection(url);

                // Don't do advanced test connection with sso login, will cause deadlock
                if (credentialLogin)
                {
                    // reset and thus require a new octane service obj
                    Run(async () => { return await OctaneServices.Reset(); }).Wait();

                    // hotfix for first time installing the plugin and clicking test connection
                    if (OctaneConfiguration.CredentialLogin == false)
                    {
                        OctaneConfiguration.CredentialLogin = true;
                    }

                    // create a new service object
                    OctaneServices.Create(OctaneConfiguration.Url,
                              OctaneConfiguration.SharedSpaceId,
                              OctaneConfiguration.WorkSpaceId);

                    await OctaneServices.GetInstance().Connect();

                    // try to get the work item root
                    await OctaneServices.GetInstance().GetAsyncWorkItemRoot();
				}

				return ConnectionSuccessful;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		public string Url
		{
			get { return url; }
			set
			{
                url = value;			
				OctaneConfiguration.Url = url;
			}
		}

		public String SsId
		{
			get
            {    
                return ssid == 0 ? "": ssid + "";
            }
			set
			{
                try
                {
                    ssid = long.Parse(value);

                    if(ssid < 0)
                    {
                        ssid = 0;
                    }
                }
                catch (Exception) {
                    ssid = 0;
                }
				
				OctaneConfiguration.SharedSpaceId = ssid;
			}
		}

        public String WsId
        {
            get
            {
                return wsid == 0 ? "" : wsid + "";
            }
            set
            {
                try
                {
                    wsid = long.Parse(value);

                    if (wsid < 0)
                    {
                        wsid = 0;
                    }
                }
                catch (Exception)
                {
                    wsid = 0;
                }

                OctaneConfiguration.WorkSpaceId = wsid;
            }
        }

        public string User
		{
			get { return user; }
			set
			{
				user = value;
				OctaneConfiguration.Username = user;
			}
		}

		private static string _encryptedPassword = null;

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

		public Boolean CredentialLogin
		{
			get
			{
				return credentialLogin;
			}
			set
			{
				credentialLogin = value;
				OctaneConfiguration.CredentialLogin = credentialLogin;
			}
		}
		public Boolean SsoLogin
		{
			get
			{
				return ssologin;
			}
			set
			{
				ssologin = value;
				OctaneConfiguration.SsoLogin = ssologin;
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
			if (!Equals(Password, _encryptedPassword))
			{
				EncryptPassword();
				_encryptedPassword = Password;
			}
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


		protected override System.Windows.UIElement Child
		{
			get
			{
				page = new ConnectionSettingsView
				{
					optionsPage = this
				};
				page.Initialize();
				return page;
			}
		}

		public void setOldCredentials()
        {
			oldUserPassConnectionInfo = new UserPassConnectionInfo(OctaneConfiguration.Username, OctaneConfiguration.Password);
			oldSsid = OctaneConfiguration.SharedSpaceId;
			oldWsid = OctaneConfiguration.WorkSpaceId;
			oldUrl = OctaneConfiguration.Url;
		}

		private void clearUserPassTextBoxes()
		{ 
			page.usernameTextBox.Text = "";
			page.passwordTextBox.Password = "";
		}
	}
}
