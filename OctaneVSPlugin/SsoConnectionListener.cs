using MicroFocus.Adm.Octane.Api.Core.Connector.Authentication;
using MicroFocus.Adm.Octane.VisualStudio.View;
using System;
using System.Diagnostics;
using System.Windows;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    class SsoConnectionListener : ConnectionListener
    {
        private BrowserDialog browserDialog;

        public SsoConnectionListener()
        {
            browserDialog = new BrowserDialog();
        }

        public void OpenBrowser(string url)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                browserDialog.Show(url);
            }));
            
        }

        public void CloseBrowser()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                browserDialog.Show(url);
            }));
        }
    }
}
