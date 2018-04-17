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

using MicroFocus.Adm.Octane.VisualStudio.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Task = System.Threading.Tasks.Task;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    public class SearchItemsViewModel : INotifyPropertyChanged
    {
        private readonly OctaneServices _octaneService;

        private IList<BaseItemViewModel> _searchResults = new List<BaseItemViewModel>();

        public SearchItemsViewModel()
        {
            _octaneService = new OctaneServices(
                OctaneConfiguration.Url,
                OctaneConfiguration.SharedSpaceId,
                OctaneConfiguration.WorkSpaceId,
                OctaneConfiguration.Username,
                OctaneConfiguration.Password);
        }

        public async Task Search(string searchFilter)
        {
            try
            {
                await _octaneService.Connect();
                var results = await _octaneService.SearchEntities(searchFilter, 5);
                var metadata = new MyWorkMetadata();
                foreach (var entity in results)
                {
                    _searchResults.Add(new BaseItemViewModel(entity, metadata));
                }
            }
            catch (Exception ex)
            {
            }
            NotifyPropertyChanged();
        }

        /// <summary>
        /// Entity fields shown in the main section of the detailed item view
        /// </summary>
        public IEnumerable<BaseItemViewModel> SearchItems
        {
            get
            {
                return new ObservableCollection<BaseItemViewModel>(_searchResults);
            }
        }

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
