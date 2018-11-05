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
            var item = sender as StackPanel;
            if (item != null)
            {
               
                if (MultiSelectVisibility == Visibility.Visible)
                {
                    CheckBox cb = (CheckBox)item.Children[0];
                    if (cb.IsChecked.Equals(true))
                    {
                        //take the selected item out of the selected items list
                        EntityList<BaseEntity> selectedEntities = ((FieldViewModel)DataContext).GetSelectedEntities();
                       
                        BaseEntityWrapper selectedEntity = (BaseEntityWrapper)item.DataContext;
                        
                        BaseEntity entityToRemove = null;
                        foreach (BaseEntity baseEntity in selectedEntities.data)
                        {
                            if(selectedEntity.Equals(new BaseEntityWrapper(baseEntity)))
                            {
                                entityToRemove = baseEntity;
                                break;
                            }
                        }
                        selectedEntities.data.Remove(entityToRemove);
                        ((FieldViewModel)EditorLabelName.DataContext).Content = selectedEntities;
                        
                        cb.IsChecked = false;
                    }
                    else
                    {
                        //insert the selected value into the selected items list
                        EntityList<BaseEntity> selectedEntities = ((FieldViewModel)DataContext).GetSelectedEntities();

                        BaseEntityWrapper entityToInsert = ((BaseEntityWrapper)item.DataContext);
                        
                        selectedEntities.data.Add(entityToInsert.BaseEntity);
                        ((FieldViewModel)EditorLabelName.DataContext).Content = selectedEntities;
                        
                        cb.IsChecked = true;
                    }

                } else
                {
                    ComboBoxPopup.IsOpen = false;
                    ((FieldViewModel)EditorLabelName.DataContext).Content = ((BaseEntityWrapper)item.DataContext).BaseEntity;
                }
            }
            
        }

        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            listView.Items.Refresh();
            ComboBoxPopup.IsOpen = true;
        }
    }


    


}
