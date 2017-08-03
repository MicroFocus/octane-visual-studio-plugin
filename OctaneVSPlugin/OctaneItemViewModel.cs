using System.Linq;
using Hpe.Nga.Api.Core.Entities;
using System.Collections.Generic;
using System.Windows.Media;

namespace Hpe.Nga.Octane.VisualStudio
{
    public class OctaneItemViewModel
    {
        private static readonly Dictionary<string, string> iconTextMap;
        private static readonly Dictionary<string, Color> iconBackgroundColorMap;

        private readonly WorkItem workItem;

        private readonly List<FieldGetterViewModel> topFields;
        private readonly List<FieldGetterViewModel> bottomFields;
        private readonly FieldGetterViewModel subTitleField;

        static OctaneItemViewModel()
        {
            iconTextMap = new Dictionary<string, string>
            {
                { WorkItem.SUBTYPE_DEFECT, "D" },
                { WorkItem.SUBTYPE_STORY, "US" }
            };
            iconBackgroundColorMap = new Dictionary<string, Color>
            {
                { WorkItem.SUBTYPE_DEFECT, Color.FromRgb(190, 102, 92) },
                { WorkItem.SUBTYPE_STORY, Color.FromRgb(218, 199, 120) }
            };
        }

        public OctaneItemViewModel(WorkItem workItem)
        {
            this.workItem = workItem;

            topFields = new List<FieldGetterViewModel>();
            bottomFields = new List<FieldGetterViewModel>();

            if (workItem.SubType == WorkItem.SUBTYPE_STORY)
            {
                subTitleField = new FieldGetterViewModel(this, "release", "Release", "No release");

                topFields.Add(new FieldGetterViewModel(this, "owner", "Owner"));
                topFields.Add(new FieldGetterViewModel(this, "phase", "Phase"));
                topFields.Add(new FieldGetterViewModel(this, "story_points", "Story Points"));

                bottomFields.Add(new FieldGetterViewModel(this, "invested_hours", "Invested Hors"));
                bottomFields.Add(new FieldGetterViewModel(this, "remaining_hours", "Remaining Hors"));
                bottomFields.Add(new FieldGetterViewModel(this, "estimated_hours", "Estimated Hors"));
            }
            else if (workItem.SubType == WorkItem.SUBTYPE_DEFECT)
            {
                subTitleField = new FieldGetterViewModel(this, "taxonomies", "Environments", "No environment");

                topFields.Add(new FieldGetterViewModel(this, "owner", "Owner"));
                topFields.Add(new FieldGetterViewModel(this, "detected_by", "Detected By"));
                topFields.Add(new FieldGetterViewModel(this, "story_points", "SP"));
                topFields.Add(new FieldGetterViewModel(this, "severity", "Severity"));

                bottomFields.Add(new FieldGetterViewModel(this, "invested_hours", "Invested Hors"));
                bottomFields.Add(new FieldGetterViewModel(this, "remaining_hours", "Remaining Hors"));
                bottomFields.Add(new FieldGetterViewModel(this, "estimated_hours", "Estimated Hors"));
            }
        }

        public WorkItem WorkItem {  get { return workItem; } }

        public long ID { get { return workItem.Id; } }
        public string Name { get { return workItem.Name; } }
        public string Phase { get { return workItem.Phase.Name; } }
        public string Description { get { return workItem.Description ?? string.Empty; } }

        public FieldGetterViewModel SubTitleField
        {
            get { return subTitleField; }
        }

        public IEnumerable<object> TopFields {
            get
            {
                return FieldsWithSeparators(topFields);
            }
        }
        public IEnumerable<object> BottomFields {
            get
            {
                return FieldsWithSeparators(bottomFields);
            }
        }

        private IEnumerable<object> FieldsWithSeparators(List<FieldGetterViewModel> fields)
        {
            foreach (FieldGetterViewModel field in fields.Take(fields.Count - 1))
            {
                yield return field;
                yield return SeparatorViewModel.Make();
            }

            yield return fields.Last();
        }

        public string IconText
        {
            get
            {
                string iconText;
                if (iconTextMap.TryGetValue(workItem.SubType, out iconText))
                    return iconText;
                else
                    return "?";
            }
        }

        public Color IconBackgroundColor
        {
            get
            {
                Color bgc;
                if(iconBackgroundColorMap.TryGetValue(workItem.SubType, out bgc))
                    return bgc;
                else
                    return Color.FromRgb(221, 221, 221);
            }
        }
    }
}
