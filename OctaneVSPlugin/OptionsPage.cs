using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Hpe.Nga.Octane.VisualStudio
{
    /// <summary>
    /// Options page represent the settings the user can set for ALM Octane plugin.
    /// This class is presented to the user as a page in Visual Studio options dialog.
    /// </summary>
    internal class OptionsPage : DialogPage {
        const string category = "Server Settings";

        private string url = string.Empty; //"http://myd-vm10629.hpeswlab.net:8081";
        private int ssid = 1001;
        private int wsid = 1002;
        private string user = string.Empty; //"sa@nga";
        private string password = string.Empty; //"Welcome1";

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            // After settings are applied we notify the main ViewModel to allow it to refresh.
            if (OctaneMyItemsViewModel.Instance != null)
            {
                OctaneMyItemsViewModel.Instance.OptionsChanged();
            }
        }

        [Category(category)]
        [DisplayName("1. Server URL")]
        [Description("Format: http://servername:8081 (do not include additional info)")]
        public string Url {
            get { return url; }
            set { url = value; }
        }

        [Category(category)]
        [DisplayName("2. Shared Space ID")]
        public int SsId {
            get { return ssid; }
            set { ssid = value; }
        }

        [Category(category)]
        [DisplayName("3. Workspace ID")]
        public int WsId {
            get { return wsid; }
            set { wsid = value; }
        }


        [Category(category)]
        [DisplayName("4. User")]
        public string User {
            get { return user; }
            set { user = value; }
        }

        [Category(category)]
        [DisplayName("5. Password")]
        [PasswordPropertyText(true)]
        public string Password {
            get { return password; }
            set { password = value; }
        }

    }
}
