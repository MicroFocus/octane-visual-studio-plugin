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

using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Info about the icon label and color.
    /// </summary>
    public class ItemIconInfo
    {
        public string ShortLabel { get; private set; }
        public Color LabelColor { get; private set; }

        public ItemIconInfo(string shortLabel, Color labelColor)
        {
            ShortLabel = shortLabel;
            LabelColor = labelColor;
        }
    }
}
