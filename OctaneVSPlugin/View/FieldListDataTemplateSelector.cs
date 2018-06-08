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

using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    public class FieldListDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element == null)
                return null;

            var field = item as FieldViewModel;
            if (field == null)
                return null;

            if (field.Metadata.GetStringValue("editable") == "True"
                && field.Metadata.GetStringValue("final") == "False"
                && (field.Metadata.FieldType == "boolean"
                    || field.Metadata.FieldType == "integer"
                    || field.Metadata.FieldType == "string"))
            {
                return element.FindResource("EditableFieldTemplate") as DataTemplate;
            }
            else
            {
                return element.FindResource("ReadOnlyFieldTemplate") as DataTemplate;
            }
        }
    }
}
