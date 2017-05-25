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
        private readonly OctaneMyItemsViewModel viewModel;
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            this.InitializeComponent();
            viewModel  = new OctaneMyItemsViewModel();
            this.DataContext = viewModel;
        }

        public void SetPackage(MainWindowPackage package)
        {
            viewModel.SetPackage(package);
        }
    }
}