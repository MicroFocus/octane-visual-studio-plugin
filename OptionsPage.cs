using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace octane_visual_studio_plugin {

    class OptionsPage : DialogPage {
        private string url = "http://myd-vm10629.hpeswlab.net:8081";
        const string category = "Server Settings";
        private int ssid = 1001;
        private int wsid = 1002;
        private string user = "sa@nga";
        private string password = "Welcome1";

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
