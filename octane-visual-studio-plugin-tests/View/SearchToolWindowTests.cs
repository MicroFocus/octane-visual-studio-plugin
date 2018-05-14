using MicroFocus.Adm.Octane.VisualStudio.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.View
{
    [TestClass]
    public class SearchToolWindowTests : BaseOctanePluginTest
    {
        [TestMethod]
        public void SearchToolWindowTests_Search_NullFilter_Success()
        {
            ValidateCaption(null, "\"\"");
        }

        [TestMethod]
        public void SearchToolWindowTests_Search_EmptyFilter_Success()
        {
            ValidateCaption(string.Empty, "\"\"");
        }

        [TestMethod]
        public void SearchToolWindowTests_Search_SpaceFilter_Success()
        {
            ValidateCaption(" ", "\"\"");
        }

        [TestMethod]
        public void SearchToolWindowTests_Search_NormalFilter_Success()
        {
            ValidateCaption("abc", "\"abc\"");
        }

        [TestMethod]
        public void SearchToolWindowTests_Search_FilterWithLimitNumberOfChars_Success()
        {
            ValidateCaption("12345678901234567890", "\"12345678901234567890\"");
        }

        [TestMethod]
        public void SearchToolWindowTests_Search_FilterWithGreaterNumberOfChars_Success()
        {
            ValidateCaption("1234567890123456789012", "\"12345678901234567890...\"");
        }

        private void ValidateCaption(string filter, string expectedCaption)
        {
            var window = new SearchToolWindow();
            window.Search(filter);
            Assert.AreEqual(expectedCaption, window.Caption, "Mismatched SearchToolWindow caption");
        }
    }
}
