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

using MicroFocus.Adm.Octane.Api.Core.Connector;
using MicroFocus.Adm.Octane.Api.Core.Connector.Authentication;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.View;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static System.Threading.Tasks.Task;
using System.Windows.Media;
using System.Windows;

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
		private int ssid = 1001;
		private int wsid = 1002;
		private string user = string.Empty;
		private string password = string.Empty;
		private bool credentialLogin = true;
		private bool ssologin = false;
		private ConnectionSettingsView page;

		protected override void OnApply(PageApplyEventArgs e)
		{

			page.SetInfoLabelText("");
			SaveSettingsToStorage();
			var result = Run(async () => { return await TestConnection(); }).Result;

			if (!result.Equals(ConnectionSuccessful))
			{
				e.ApplyBehavior = ApplyKind.CancelNoNavigate;
			}

			else
			{
				// reset and thus require a new octane service obj
				Run(async () => { return await OctaneServices.Reset(); }).Wait();

				// create a new service object
				OctaneServices.Create(OctaneConfiguration.Url,
						  OctaneConfiguration.SharedSpaceId,
						  OctaneConfiguration.WorkSpaceId);

				Run(async () => { await OctaneServices.GetInstance().Connect(); }).Wait();

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

			page.SetInfoLabelText(result);
			base.OnApply(e);
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

		public int SsId
		{
			get { return ssid; }
			set
			{
				ssid = value;
				OctaneConfiguration.SharedSpaceId = ssid;
			}
		}

		public int WsId
		{
			get { return wsid; }
			set
			{
				wsid = value;
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
		public bool CredentialLogin
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
		public bool SsoLogin
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
	}
}
