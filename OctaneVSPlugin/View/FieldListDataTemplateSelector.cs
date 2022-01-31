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

using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using System;
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

            string templateName = "ReadOnlyFieldTemplate";
            if (field.Metadata.GetStringValue("editable") == "True"
                && field.Metadata.GetStringValue("final") == "False")
            {
                switch (field.Metadata.FieldType)
                {
                    case "boolean":
                        templateName = "EditableBooleanFieldTemplate";
                        break;
                    case "integer":
                    case "float":
                    case "string":
                        templateName = "EditableFieldTemplate";
                        break;
                    case "date_time":
                        if (field.Content != null)
                        {
                            templateName = "EditableDateFieldTemplate";
                        } else
                        {
                            field.Content = DateTime.UtcNow;
                            templateName = "EditableDateFieldTemplateEmpty";
                        }
                        break;
                    case "reference":
                        if (field.IsMultiple)
                        {
                            if (field.IsMoreThanOneTarget || field.FieldNotCompatible)
                            {
                                templateName = "ReadOnlyFieldTemplate";
                                break;
                            }
                            templateName = "EditableReferenceFieldTemplateMultiple";
                            
                        }
                        else
                        {
                            if (field.IsMoreThanOneTarget || field.FieldNotCompatible)
                            {
                                templateName = "ReadOnlyFieldTemplate";
                                break;
                            }
                            templateName = "EditableReferenceFieldTemplateSimple";
                        }
                        break;
                    default:
                        templateName = "ReadOnlyFieldTemplate";
                        break;
                }
            }

            return element.FindResource(templateName) as DataTemplate;
        }
    }
}
