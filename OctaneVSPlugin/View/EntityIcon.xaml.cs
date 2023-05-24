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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// Interaction logic for EntityIcon.xaml
    /// </summary>
    public partial class EntityIcon : UserControl
    {
        public EntityIcon()
        {
            InitializeComponent();
        }

        public Color IconColor
        {
            get { return (Color)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }

        public static DependencyProperty IconColorProperty =
           DependencyProperty.Register("IconColor", typeof(Color), typeof(EntityIcon), new PropertyMetadata(Colors.LightBlue));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
               DependencyProperty.Register("Text", typeof(string), typeof(EntityIcon), new PropertyMetadata("LE"));
    }
}
