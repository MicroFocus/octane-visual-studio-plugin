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
