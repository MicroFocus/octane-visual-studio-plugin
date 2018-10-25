using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// Interaction logic for EntityComboBox.xaml
    /// </summary>
    public partial class EntityComboBox : UserControl
    {
        public Visibility HasSearchBox
        {
            get { return SearchBox.Visibility; }
            set { SearchBox.Visibility = value;}
        }

        private Visibility _MultiSelectVisibility;
        
        public Visibility MultiSelectVisibility {
            get { return _MultiSelectVisibility; }
            set { _MultiSelectVisibility = value; }
        }

        public EntityComboBox()
        {
            InitializeComponent();  
            
        }

        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            ComboBoxPopup.IsOpen = true;
        }
    }


    


}
