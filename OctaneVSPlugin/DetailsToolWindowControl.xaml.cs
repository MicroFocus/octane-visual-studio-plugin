//------------------------------------------------------------------------------
// <copyright file="OctaneToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace MicroFocus.Adm.Octane.VisualStudio
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for OctaneToolWindowControl.
    /// </summary>
    public partial class OctaneToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OctaneToolWindowControl"/> class.
        /// </summary>
        public OctaneToolWindowControl()
        {
            this.InitializeComponent();
            DataContextChanged += OctaneToolWindowControl_DataContextChanged;
        }

        private void OctaneToolWindowControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var itemViewModel = DataContext as OctaneItemViewModel;
            if (itemViewModel != null)
            {
                string octaneImageBaseUrl = string.Format("{0}/api/shared_spaces/{1}/workspaces/{2}/attachments/",
                    OctaneMyItemsViewModel.Instance.Package.AlmUrl,
                    OctaneMyItemsViewModel.Instance.Package.SharedSpaceId,
                    OctaneMyItemsViewModel.Instance.Package.WorkSpaceId);

                string htmlWithImageUrls = itemViewModel.Description.Replace("file://[IMAGE_BASE_PATH_PLACEHOLDER]", octaneImageBaseUrl);

                if (!string.IsNullOrWhiteSpace(htmlWithImageUrls))
                {
                    DescBrowser.NavigateToString(htmlWithImageUrls);
                }
            }
        }

        private void DescBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
            {
                e.Cancel = true;
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
        }
    }
}