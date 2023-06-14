/*!
* Copyright 2017 – 2023 Open Text.
*
* The only warranties for products and services of Open Text and
* its affiliates and licensors (“Open Text”) are as may be set
* forth in the express warranty statements accompanying such products
* and services. Nothing herein should be construed as constituting an
* additional warranty. Open Text shall not be liable for technical or
* editorial errors or omissions contained herein. The information
* contained herein is subject to change without notice.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using MicroFocus.Adm.Octane.VisualStudio.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.View
{
    /// <summary>
    /// Test class for <see cref="SearchToolWindow"/>
    /// </summary>
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
