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

using MicroFocus.Adm.Octane.Api.Core.Connector.Exceptions;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows;


namespace MicroFocus.Adm.Octane.VisualStudio.View
{

    /// <summary>
    /// Interaction logic for BusinessErrorDialog.xaml
    /// </summary>
    public partial class BusinessErrorDialog : DialogWindow
    {
        private DetailedItemViewModel viewModel;

        public BusinessErrorDialog(DetailedItemViewModel viewModel, MqmRestException ex) : base()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            errorMessage.Content = ex.Message;
            errorCode.Content = ex.ErrorCode;
            httpStatus.Content = ex.StatusCode;
            correlationId.Content = ex.CorrelationInfo;
            stackTrace.Text = ex.StackTrace;
            this.viewModel = viewModel;
        }

        public void DoRefresh_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RefreshCommand.Execute(sender);
            this.Close();
        }

        public void OpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OpenInBrowserCommand.Execute(sender);
            this.Close();
        }

        public void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
