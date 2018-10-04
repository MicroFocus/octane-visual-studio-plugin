using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{

    /// <summary>
    /// Interaction logic for BusinessErrorDialog.xaml
    /// </summary>
    public partial class BusinessErrorDialog : DialogWindow
    {
        private DetailedItemViewModel viewModel;

        public BusinessErrorDialog(DetailedItemViewModel viewModel, Exception ex) : base()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            errorMessage.Content = ex.Message;
            this.viewModel = viewModel;
        }

        public void DoRefresh_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RefreshCommand.Execute(sender);
            this.Close();
        }

        public void OpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OpenInBrowserCommand.Execute(sender);
            this.Close();
        }

        public void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
