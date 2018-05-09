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
using System;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Common
{
    /// <summary>
    /// Test class for <see cref=" GreaterThanZeroConverter"/>
    /// </summary>
    [TestClass]
    public class GreaterThanZeroConverterTests
    {
        private readonly GreaterThanZeroConverter _converter = new GreaterThanZeroConverter();

        #region Convert

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GreaterThanValueConverterTests_Convert_NullValue_Throws()
        {
            _converter.Convert(null, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void GreaterThanValueConverterTests_Convert_ObjectValue_Throws()
        {
            _converter.Convert("test", null, null, null);
        }

        [TestMethod]
        public void GreaterThanValueConverterTests_Convert_NumericStringValue_Success()
        {
            Assert.IsTrue((bool)_converter.Convert("123", null, null, null), "Invalid convert result");
        }

        [TestMethod]
        public void GreaterThanValueConverterTests_Convert_GreaterValue_Success()
        {
            Assert.IsTrue((bool)_converter.Convert(4, null, null, null), "Invalid convert result");
        }

        [TestMethod]
        public void GreaterThanValueConverterTests_Convert_ZeroValue_Success()
        {
            Assert.IsFalse((bool)_converter.Convert(0, null, null, null), "Invalid convert result");
        }

        [TestMethod]
        public void GreaterThanValueConverterTests_Convert_SmallerValue_Success()
        {
            Assert.IsFalse((bool)_converter.Convert(-2, null, null, null), "Invalid convert result");
        }

        #endregion

        #region ConvertBack

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void GreaterThanValueConverterTests_ConvertBack_Throws()
        {
            _converter.ConvertBack(null, null, null, null);
        }

        #endregion
    }
}
