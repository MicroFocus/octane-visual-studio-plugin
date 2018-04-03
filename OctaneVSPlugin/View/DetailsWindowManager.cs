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

using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.Shell.Interop;
using octane_visual_studio_plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// Manager for keeping track of all the opened<see cref="DetailsToolWindow"/>
    /// </summary>
    internal static class DetailsWindowManager
    {
        private static object Lock = new object();

        private static int WindowCounter = 0;
        private static readonly Dictionary<string, WindowInfo> OpenedDetailWindows = new Dictionary<string, WindowInfo>();

        /// <summary>
        /// Return the details window for the given entity
        /// </summary>
        internal static DetailsToolWindow ObtainDetailsWindow(MainWindowPackage package, BaseEntity entity)
        {
            var uniqueId = GetUniqueIdentifier(entity);

            bool windowAlreadyOpened = false;
            int windowId = WindowCounter + 1;

            lock (Lock)
            {
                WindowInfo windowInfo;
                if (OpenedDetailWindows.TryGetValue(uniqueId, out windowInfo))
                {
                    windowAlreadyOpened = true;
                    windowId = windowInfo.WindowId;
                }

                // open the existing window with the current windowId
                // if a window wasn't found, then a new one is created
                DetailsToolWindow toolWindow = (DetailsToolWindow)package.FindToolWindow(typeof(DetailsToolWindow), windowId, true);
                if (toolWindow?.Frame == null)
                {
                    throw new NotSupportedException("Cannot create tool window");
                }

                if (!windowAlreadyOpened)
                {
                    OpenedDetailWindows.Add(uniqueId, new WindowInfo(++WindowCounter, toolWindow));
                }

                return toolWindow;
            }
        }

        private static string GetUniqueIdentifier(BaseEntity entity)
        {
            return Utility.GetConcreteEntityType(entity) + entity.Id;
        }

        /// <summary>
        /// Unregister the given details window from the window manager
        /// </summary>
        internal static void UnregisterDetailsWindow(DetailsToolWindow detailWindow)
        {
            var windowControl = detailWindow.Content as OctaneToolWindowControl;
            if (windowControl == null)
                return;

            var viewModel = windowControl.DataContext as DetailedItemViewModel;
            if (viewModel == null)
                return;

            var uniqueId = GetUniqueIdentifier(viewModel.Entity);
            lock (Lock)
            {
                OpenedDetailWindows.Remove(uniqueId);
            }
        }

        /// <summary>
        /// Manually close all the currently opened details windows
        /// </summary>
        internal static void CloseAllDetailsWindows()
        {
            lock (Lock)
            {
                foreach (var windowInfo in OpenedDetailWindows.Values.ToList())
                {
                    var frame = windowInfo.DetailsWindow.Frame as IVsWindowFrame;
                    if (frame != null)
                        frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                }
                OpenedDetailWindows.Clear();
            }
        }

        private struct WindowInfo
        {
            public int WindowId { get; }
            public DetailsToolWindow DetailsWindow { get; }

            public WindowInfo(int windowId, DetailsToolWindow detailsWindow)
            {
                WindowId = windowId;
                DetailsWindow = detailsWindow;
            }
        }
    }
}
