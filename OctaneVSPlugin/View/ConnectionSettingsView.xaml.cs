﻿using System;
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
    /// Interaction logic for ConnectionSettingsView.xaml
    /// </summary>
    public partial class ConnectionSettingsView : UserControl
    {
        public ConnectionSettingsView()
        {
            InitializeComponent();
        }

        internal ConnectionSettings optionsPage;

        public void Initialize()
        {
            //textBox1.Text = optionsPage.OptionString;
            //populate the details from persistance url shid wid user and pass
            this.DataContext = optionsPage;
        }

        private void TestConnection(object sender, RoutedEventArgs e)
        {
            // do something
        }

        private void ClearSettings(object sender, RoutedEventArgs e)
        {
            // do something
        }
    }
}
