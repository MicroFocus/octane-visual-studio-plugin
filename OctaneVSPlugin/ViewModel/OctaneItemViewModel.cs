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

using MicroFocus.Adm.Octane.Api.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    public class OctaneItemViewModel : BaseItemViewModel
    {
        private readonly List<FieldViewModel> topFields;
        private readonly List<FieldViewModel> bottomFields;
        private readonly FieldViewModel subTitleField;

        public OctaneItemViewModel(BaseEntity entity, MyWorkMetadata myWorkMetadata)
            : base(entity, myWorkMetadata)
        {
            topFields = new List<FieldViewModel>();
            bottomFields = new List<FieldViewModel>();

            subTitleField = new FieldViewModel(Entity, myWorkMetadata.GetSubTitleFieldInfo(entity));

            foreach (FieldInfo fieldInfo in myWorkMetadata.GetTopFieldsInfo(entity))
            {
                topFields.Add(new FieldViewModel(Entity, fieldInfo));
            }

            foreach (FieldInfo fieldInfo in myWorkMetadata.GetBottomFieldsInfo(entity))
            {
                bottomFields.Add(new FieldViewModel(Entity, fieldInfo));
            }
        }

        public virtual bool VisibleID { get { return true; } }

        public string TypeName
        {
            get { return Entity.TypeName; }
        }

        public string SubType
        {
            get { return Entity.GetStringValue(CommonFields.SUB_TYPE); }
        }

        public string CommitMessage
        {
            get
            {
                string message = string.Format("{0} #{1}: ", MyWorkMetadata.GetCommitMessageTypeName(Entity), ID);
                return message;
            }
        }

        public bool IsSupportCopyCommitMessage
        {
            get { return MyWorkMetadata.IsSupportCopyCommitMessage(Entity); }
        }

        public FieldViewModel SubTitleField
        {
            get { return subTitleField; }
        }

        public IEnumerable<object> TopFields
        {
            get
            {
                return FieldsWithSeparators(topFields);
            }
        }
        public IEnumerable<object> BottomFields
        {
            get
            {
                return FieldsWithSeparators(bottomFields);
            }
        }

        private IEnumerable<object> FieldsWithSeparators(List<FieldViewModel> fields)
        {
            // Handle the case there are no fields so we don't need any seperators.
            if (fields.Count == 0)
            {
                yield break;
            }

            foreach (FieldViewModel field in fields.Take(fields.Count - 1))
            {
                yield return field;
                yield return SeparatorViewModel.Make();
            }

            yield return fields.Last();
        }
    }
}
