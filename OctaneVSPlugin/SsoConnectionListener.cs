using MicroFocus.Adm.Octane.Api.Core.Connector.Authentication;
using System;
using System.Diagnostics;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    class SsoConnectionListener : ConnectionListener
    {
        public void OpenBrowser(string url)
        {
            // Open the URL in the user's default browser.
            Process.Start(url);
        }
    }
}
