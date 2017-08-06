using System;
using System.Windows.Input;
using octane_visual_studio_plugin;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Hpe.Nga.Api.Core.Entities;
using System.ComponentModel;

namespace Hpe.Nga.Octane.VisualStudio
{
    public class OctaneMyItemsViewModel : INotifyPropertyChanged
    {
        private static OctaneMyItemsViewModel instance;

        private DelegatedCommand refreshCommand;
        private DelegatedCommand openOctaneOptionsDialogCommand;
        private MainWindowPackage package;

        private MainWindowMode mode;
        private ObservableCollection<OctaneItemViewModel> myItems;

        /// <summary>
        /// Store the exception message from load items.
        /// </summary>
        private string lastExceptionMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public OctaneMyItemsViewModel()
        {
            instance = this;

            refreshCommand = new DelegatedCommand(Refresh);
            openOctaneOptionsDialogCommand = new DelegatedCommand(OpenOctaneOptionsDialog);
            myItems = new ObservableCollection<OctaneItemViewModel>();
            mode = MainWindowMode.FirstTime;
        }

        public static OctaneMyItemsViewModel Instance
        {
            get { return instance; }
        }

        public MainWindowMode Mode
        {
            get { return mode; }
            private set
            {
                mode = value;
                NotifyPropertyChanged("Mode");
            }
        }

        public ICommand RefreshCommand
        {
            get { return refreshCommand; }
        }

        public ICommand OpenOctaneOptionsDialogCommand
        {
            get { return openOctaneOptionsDialogCommand; }
        }

        public IEnumerable<OctaneItemViewModel> MyItems
        {
            get { return myItems; }
        }

        public string LastExceptionMessage
        {
            get { return lastExceptionMessage; }
            set
            {
                lastExceptionMessage = value;
                NotifyPropertyChanged("LastExceptionMessage");
            }
        }

        private void Refresh(object parameter)
        {
            LoadMyItems();
        }

        private void OpenOctaneOptionsDialog(object parameter)
        {
            package.ShowOptionPage(typeof(OptionsPage));
        }

        /// <summary>
        /// This method is called after the options are changed.
        /// </summary>
        internal void OptionsChanged()
        {
            LoadMyItems();
        }

        internal async void LoadMyItems()
        {
            if (string.IsNullOrEmpty(package.AlmUrl))
            {
                // No URL which means it's the first time use.
                Mode = MainWindowMode.FirstTime;
                return;
            }

            Mode = MainWindowMode.LoadingItems;

            try
            {
                OctaneServices octane = new OctaneServices(package.AlmUrl, package.SharedSpaceId, package.WorkSpaceId, package.AlmUsername, package.AlmPassword);
                await octane.Connect();

                myItems.Clear();

                IList<WorkItem> items = await octane.GetMyItems(new HashSet<string>(new string[] { WorkItem.SUBTYPE_DEFECT, WorkItem.SUBTYPE_STORY }));
                foreach (WorkItem workItem in items)
                {
                    myItems.Add(new OctaneItemViewModel(workItem));
                }

                Mode = MainWindowMode.ItemsLoaded;
            }
            catch (Exception ex)
            {
                Mode = MainWindowMode.FailToLoad;
                LastExceptionMessage = ex.Message;
            }
        }

        internal void SetPackage(MainWindowPackage package)
        {
            this.package = package;
            LoadMyItems();
        }

        internal MainWindowPackage Package
        {
            get { return package; }
        }

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
