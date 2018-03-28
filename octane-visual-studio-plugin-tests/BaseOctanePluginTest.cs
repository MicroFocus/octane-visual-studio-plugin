using MicroFocus.Adm.Octane.Api.Core.Tests;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests
{
    public abstract class BaseOctanePluginTest : BaseTest
    {
        protected readonly MyWorkMetadata MyWorkMetadata = new MyWorkMetadata();

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            InitConnection(context);

            OctaneConfiguration.Url = host;
            OctaneConfiguration.Username = userName;
            OctaneConfiguration.Password = password;
            OctaneConfiguration.WorkSpaceId = workspaceContext.WorkspaceId;
            OctaneConfiguration.SharedSpaceId = sharedSpaceContext.SharedSpaceId;
        }
    }
}
