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

using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{

	/// <summary>
	/// Interaction logic for BrowserDialog.xaml
	/// </summary>
	public partial class BrowserDialog : DialogWindow
	{
		public string Url { get; set; }

		public BrowserDialog()
		{
			InitializeComponent();
		}

		private void hyperlink_Click(object sender, RoutedEventArgs e)
		{
			// Open the URL in the user's default browser.
			Process.Start(Url);
		}

        public void Show(string Url)
        {
            this.Url = Url;
            browser.Navigate(new Uri(Url));
            Show();
        }

        public void Close()
        {
            base.Close();
        }
	}
}