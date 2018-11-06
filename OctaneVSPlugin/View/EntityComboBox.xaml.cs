using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.Api.Core.Services;
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

        public ObservableCollection<BaseEntity> SelectedValues;

        private void SelectionHandler(object sender, MouseButtonEventArgs e)
        {
            var item = ((sender as ListView).SelectedItem as BaseEntityWrapper);
            
            if (item != null)
            {
                if (MultiSelectVisibility == Visibility.Visible)
                {
                    if (item.IsSelected.Equals(true))
                    {
                        //take the selected item out of the selected items list
                        EntityList<BaseEntity> selectedEntities = ((FieldViewModel)DataContext).GetSelectedEntities();
                        
                        BaseEntity entityToRemove = null;
                        foreach (BaseEntity baseEntity in selectedEntities.data)
                        {
                            if(item.Equals(new BaseEntityWrapper(baseEntity)))
                            {
                                entityToRemove = baseEntity;
                                break;
                            }
                        }
                        selectedEntities.data.Remove(entityToRemove);
                        ((FieldViewModel)EditorLabelName.DataContext).Content = selectedEntities;

                        item.IsSelected = false;
                    }
                    else
                    {
                        //insert the selected value into the selected items list
                        EntityList<BaseEntity> selectedEntities = ((FieldViewModel)DataContext).GetSelectedEntities();
                        
                        selectedEntities.data.Add(item.BaseEntity);
                        ((FieldViewModel)EditorLabelName.DataContext).Content = selectedEntities;

                        item.IsSelected = true;
                    }

                } else
                {
                    ComboBoxPopup.IsOpen = false;
                    ((FieldViewModel)EditorLabelName.DataContext).Content = item.BaseEntity;
                }
                listView.Items.Refresh();
            }
            
        }

        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            listView.Items.Refresh();
            ComboBoxPopup.IsOpen = true;
        }
    }


    


}
