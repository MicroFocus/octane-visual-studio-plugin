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

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// Represents the modes in which the main window can be in.
    /// </summary>
    public enum MainWindowMode
    {
        /// <summary>
        /// This mode is set on the first use of this plugin and guide the user how to configure the connection to Octane.
        /// </summary>
        FirstTime,

        /// <summary>
        /// This mode is set during loading of the items.
        /// </summary>
        LoadingItems,

        /// <summary>
        /// This mode is set after the items have been loaded.
        /// </summary>
        ItemsLoaded,

        /// <summary>
        /// This mode is set when we fail to load the items usually due to network issue or authentication issues.
        /// </summary>
        FailToLoad
    }
}
