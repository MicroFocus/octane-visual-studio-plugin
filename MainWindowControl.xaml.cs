//------------------------------------------------------------------------------
// <copyright file="MainWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Hpe.Nga.Octane.VisualStudio
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using Hpe.Nga.Api.Core.Connector;
    using Hpe.Nga.Api.Core.Entities;
    using Hpe.Nga.Api.Core.Services;
    using System;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using octane_visual_studio_plugin;

    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    public partial class MainWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            this.InitializeComponent();
        }

        public MainWindowPackage Package { get; internal set; }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OctaneServices octane = new OctaneServices(Package.AlmUrl, Package.SharedSpaceId, Package.WorkSpaceId, Package.AlmUsername, Package.AlmPassword);
            octane.Connect();
            results.Items.Clear();
            try
            {
                var items = octane.GetMyItems();
                foreach (var item in items)
                {
                    results.Items.Add(item.Id + ": " + item.Name + " (" + item.Phase.Name + ")" );
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error!");
                Console.WriteLine(ex);
            }

        }

    }
}