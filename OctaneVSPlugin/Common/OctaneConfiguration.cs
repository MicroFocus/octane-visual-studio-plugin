﻿/*!
* © Copyright 2017-2022 Micro Focus or one of its affiliates.
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

using MicroFocus.Adm.Octane.Api.Core.Connector.Authentication;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Container for configurations used to connect to Octane
    /// </summary>
    internal static class OctaneConfiguration
    {
        internal static string Url { get; set; }

        internal static string Username { get; set; }

        internal static string Password { get; set; }

        internal static long WorkSpaceId { get; set; }

        internal static long SharedSpaceId { get; set; }

        internal static bool CredentialLogin { get; set; }

        internal static bool SsoLogin { get; set; }
    }
}
