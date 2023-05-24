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

using MicroFocus.Adm.Octane.VisualStudio.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Web;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// View model for SearchToolWindow
    /// </summary>
    public class SearchItemsViewModel : INotifyPropertyChanged
    {
        private readonly string _searchFilter;
        private readonly IList<BaseItemViewModel> _searchResults = new List<BaseItemViewModel>();

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchItemsViewModel(string searchFilter)
        {
            if (searchFilter == null)
            {
                throw new ArgumentNullException(nameof(searchFilter));
            }

            _searchFilter = searchFilter.Trim();

            _searchFilter = Uri.EscapeDataString(HttpUtility.JavaScriptStringEncode(searchFilter));

            RefreshCommand = new DelegatedCommand(Refresh);

            Mode = WindowMode.Loading;

        }

        /// <summary>
        /// Search for all entities satisfying the criteria
        /// </summary>
        public async Task SearchAsync()
        {
            try
            {
                _searchResults.Clear();

                if (string.IsNullOrEmpty(_searchFilter))
                {
                    Mode = WindowMode.Loaded;
                    return;
                }

                OctaneServices octaneService = OctaneServices.GetInstance();

                var results = await octaneService.SearchEntities(_searchFilter, 20);
                foreach (var entity in results)
                {
                    // TODO - invetigate showing in bold the matching sections in Name and Description
                    entity.Name = Utility.StripHtml(entity.Name);
                    entity.SetValue(CommonFields.Description, Utility.StripHtml(entity.GetStringValue(CommonFields.Description)));

                    _searchResults.Add(new BaseItemViewModel(entity));
                }

                Mode = WindowMode.Loaded;
            }
            catch (Exception ex)
            {
                Mode = WindowMode.FailedToLoad;
                ErrorMessage = ex.Message;
            }
            finally
            {
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// The current mode of the view model
        /// </summary>
        public WindowMode Mode { get; private set; }

        /// <summary>
        /// Message displayed to the user in case of error
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Entity fields shown in the main section of the detailed item view
        /// </summary>
        public IEnumerable<BaseItemViewModel> SearchItems
        {
            get { return new ObservableCollection<BaseItemViewModel>(_searchResults); }
        }

        #region Refresh

        /// <summary>
        /// Refresh command using the same filter
        /// </summary>
        public ICommand RefreshCommand { get; }

        private async void Refresh(object param)
        {
            Mode = WindowMode.Loading;
            NotifyPropertyChanged("Mode");

            await SearchAsync();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void NotifyPropertyChanged(string propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }
}
