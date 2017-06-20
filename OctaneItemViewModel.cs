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
                topFields.Add(new FieldGetterViewModel(this, "phase", "Phase"));
                topFields.Add(new FieldGetterViewModel(this, "story_points", "Story Points"));

                bottomFields.Add(new FieldGetterViewModel(this, "invested_hours", "Invested Hors"));
                bottomFields.Add(new FieldGetterViewModel(this, "remaining_hours", "Remaining Hors"));
                bottomFields.Add(new FieldGetterViewModel(this, "estimated_hours", "Estimated Hors"));
            }
        }

        public WorkItem WorkItem {  get { return workItem; } }

        public long ID { get { return workItem.Id; } }
        public string Name { get { return workItem.Name; } }
        public string Phase { get { return workItem.Phase.Name; } }

        public List<FieldGetterViewModel> TopFields {
            get
            {
                return topFields;
            }
        }
        public List<FieldGetterViewModel> BottomFields { get
            {
                return bottomFields;
            }
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
