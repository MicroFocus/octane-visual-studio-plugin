using System;
using System.Windows.Input;
using octane_visual_studio_plugin;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Hpe.Nga.Api.Core.Entities;

namespace Hpe.Nga.Octane.VisualStudio
{
    public class OctaneMyItemsViewModel
    {
        private DelegatedCommand refreshCommand;
        private MainWindowPackage package;

        private ObservableCollection<OctaneItemViewModel> myItems;

        public OctaneMyItemsViewModel()
        {
            refreshCommand = new DelegatedCommand(Refresh);
            myItems = new ObservableCollection<OctaneItemViewModel>();
        }

        public ICommand RefreshCommand
        {
            get { return refreshCommand; }
        }

        public IEnumerable<OctaneItemViewModel> MyItems
        {
            get { return myItems; }
        }

        private void Refresh(object parameter)
        {
            LoadMyItems();
        }

        private void LoadMyItems()
        {
            OctaneServices octane = new OctaneServices(package.AlmUrl, package.SharedSpaceId, package.WorkSpaceId, package.AlmUsername, package.AlmPassword);
            octane.Connect();

            myItems.Clear();

            try
            {
                var items = octane.GetMyItems();
                foreach (WorkItem workItem in items)
                {
                    myItems.Add(new OctaneItemViewModel(workItem));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error!");
                Console.WriteLine(ex);
            }
        }

        internal void SetPackage(MainWindowPackage package)
        {
            this.package = package;
            LoadMyItems();
        }
    }
}
