/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
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

using MicroFocus.Adm.Octane.VisualStudio.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.View
{
    /// <summary>
    /// Test class for <see cref="OptionsPage"/>
    /// </summary>
    [TestClass]
    public class OptionsPageTests
    {
        [TestMethod]
        public void OptionsPageTests_Url_ValidateSetter_Success()
        {
            const string expectedUrl = "abc";
            var optionsPage = new OptionsPage { Url = expectedUrl };

            Assert.AreEqual(expectedUrl, optionsPage.Url, "Mismatched options page url");
            Assert.AreEqual(optionsPage.Url, OctaneConfiguration.Url, "Mismatched OctaneConfiguration url");
        }

        [TestMethod]
        public void OptionsPageTests_SsId_ValidateSetter_Success()
        {
            const int expectedSdId = 100;
            var optionsPage = new OptionsPage { SsId = expectedSdId };

            Assert.AreEqual(expectedSdId, optionsPage.SsId, "Mismatched options page ssid");
            Assert.AreEqual(optionsPage.SsId, OctaneConfiguration.SharedSpaceId, "Mismatched OctaneConfiguration ssid");
        }

        [TestMethod]
        public void OptionsPageTests_WsId_ValidateSetter_Success()
        {
            const int expectedWsId = 100;
            var optionsPage = new OptionsPage { WsId = expectedWsId };

            Assert.AreEqual(expectedWsId, optionsPage.WsId, "Mismatched options page wsid");
            Assert.AreEqual(optionsPage.WsId, OctaneConfiguration.WorkSpaceId, "Mismatched OctaneConfiguration wsid");
        }

        [TestMethod]
        public void OptionsPageTests_User_ValidateSetter_Success()
        {
            const string expectedUser = "username";
            var optionsPage = new OptionsPage { User = expectedUser };

            Assert.AreEqual(expectedUser, optionsPage.User, "Mismatched options page user");
            Assert.AreEqual(optionsPage.User, OctaneConfiguration.Username, "Mismatched OctaneConfiguration user");
        }

        [TestMethod]
        public void OptionsPageTests_Password_ValidateSetter_Success()
        {
            const string expectedPassword = "password";
            var optionsPage = new OptionsPage { Password = expectedPassword };

            Assert.AreEqual(expectedPassword, optionsPage.Password, "Mismatched options page password");
            Assert.AreEqual(optionsPage.Password, OctaneConfiguration.Password, "Mismatched OctaneConfiguration password");
        }
    }
}
