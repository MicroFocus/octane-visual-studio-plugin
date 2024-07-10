﻿/*!
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

using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using System;
using System.IO;
using System.Security;

/// <summary>
/// Control FEATURE_BROWSER_EMULATION registry key to make sure the WebBrowser control uses the latest version of IE
/// </summary>
namespace MicroFocus.Adm.Octane.VisualStudio.View
{
	class EmbeddedBrowserUtil
	{
		public enum BrowserEmulationVersion
		{
			Default = 0,
			Version7 = 7000,
			Version8 = 8000,
			Version8Standards = 8888,
			Version9 = 9000,
			Version9Standards = 9999,
			Version10 = 10000,
			Version10Standards = 10001,
			Version11 = 11000,
			Version11Edge = 11001
		}

		private const string InternetExplorerRootKey = @"Software\Microsoft\Internet Explorer";

		private const string BrowserEmulationKey = InternetExplorerRootKey + @"\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

		private const string ExtensionName = "Core Software Delivery Platform Visual Studio Plugin";

		public static BrowserEmulationVersion GetBrowserEmulationVersion()
		{
			BrowserEmulationVersion result;

			result = BrowserEmulationVersion.Default;

			try
			{
				RegistryKey key;

				key = Registry.CurrentUser.OpenSubKey(BrowserEmulationKey, true);
				if (key != null)
				{
					string programName;
					object value;

					programName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
					value = key.GetValue(programName, null);

					if (value != null)
					{
						result = (BrowserEmulationVersion)Convert.ToInt32(value);
					}
				}
			}
			catch (SecurityException)
			{
				// The user does not have the permissions required to read from the registry key.
			}
			catch (UnauthorizedAccessException)
			{
				// The user does not have the necessary registry rights.
			}

			return result;
		}

		public static bool SetBrowserEmulationVersion(BrowserEmulationVersion browserEmulationVersion)
		{
			bool result;

			result = false;

			try
			{
				RegistryKey key;

				key = Registry.CurrentUser.OpenSubKey(BrowserEmulationKey, true);

				if (key != null)
				{
					string programName;

					programName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

					if (browserEmulationVersion != BrowserEmulationVersion.Default)
					{
						// if it's a valid value, update or create the value
						key.SetValue(programName, (int)browserEmulationVersion, RegistryValueKind.DWord);
					}
					else
					{
						// otherwise, remove the existing value
						key.DeleteValue(programName, false);
					}

					result = true;
				}
			}
			catch (Exception)
			{
				ActivityLog.LogWarning(ExtensionName, "Failed to set the registry key for FEATURE_BROWSER_EMULATION");
			}

			return result;
		}

		public static bool SetBrowserEmulationVersionToLatestIE()
		{
			int ieVersion;
			BrowserEmulationVersion emulationCode;

			ieVersion = GetInternetExplorerMajorVersion();

			if (ieVersion >= 11)
			{
				emulationCode = BrowserEmulationVersion.Version11;
			}
			else
			{
				switch (ieVersion)
				{
					case 10:
						emulationCode = BrowserEmulationVersion.Version10;
						break;
					case 9:
						emulationCode = BrowserEmulationVersion.Version9;
						break;
					case 8:
						emulationCode = BrowserEmulationVersion.Version8;
						break;
					default:
						emulationCode = BrowserEmulationVersion.Version7;
						break;
				}
			}

			return SetBrowserEmulationVersion(emulationCode);
		}

		public static int GetInternetExplorerMajorVersion()
		{
			int result;

			result = 0;

			try
			{
				RegistryKey key;

				key = Registry.LocalMachine.OpenSubKey(InternetExplorerRootKey);

				if (key != null)
				{
					object value;

					value = key.GetValue("svcVersion", null) ?? key.GetValue("Version", null);

					if (value != null)
					{
						string version;
						int separator;

						version = value.ToString();
						separator = version.IndexOf('.');
						if (separator != -1)
						{
							int.TryParse(version.Substring(0, separator), out result);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// The user does not have the permissions required to read from the registry key.
				ActivityLog.LogError(ExtensionName, ex.Message);
			}

			return result;
		}

		public static bool IsBrowserEmulationSet()
		{
			return GetBrowserEmulationVersion() != BrowserEmulationVersion.Default;
		}

	}
}