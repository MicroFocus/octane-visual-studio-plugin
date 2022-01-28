﻿/*!
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

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// Represents the modes in which a window can be in.
    /// </summary>
    public enum WindowMode
    {
        /// <summary>
        /// This mode is set during loading of the window.
        /// </summary>
        Loading,

        ///<summary>
        /// This mode is set during loading of the comments section
        /// </summary>
        LoadingComments,

        /// <summary>
        /// This mode is set after the comments are loaded
        /// </summary>
        LoadedComments,

        /// <summary>
        /// This mode is set after the window has been loaded.
        /// </summary>
        Loaded,

        /// <summary>
        /// This mode is set when we fail to load the window.
        /// </summary>
        FailedToLoad
    }
}
