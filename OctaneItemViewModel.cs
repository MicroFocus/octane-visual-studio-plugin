using Hpe.Nga.Api.Core.Entities;
using System.Collections.Generic;
using System.Windows.Media;

namespace Hpe.Nga.Octane.VisualStudio
{
    public class OctaneItemViewModel
    {
        private readonly WorkItem workItem;

        private static readonly Dictionary<string, string> iconTextMap;
        private static readonly Dictionary<string, Color> iconBackgroundColorMap;

        static OctaneItemViewModel()
        {
            iconTextMap = new Dictionary<string, string>();
            iconTextMap.Add(WorkItem.SUBTYPE_DEFECT, "D");
            iconTextMap.Add(WorkItem.SUBTYPE_FEATURE, "F");

            iconBackgroundColorMap = new Dictionary<string, Color>();
            iconBackgroundColorMap.Add(WorkItem.SUBTYPE_DEFECT, Color.FromArgb(255, 190, 102, 92));
            iconBackgroundColorMap.Add(WorkItem.SUBTYPE_FEATURE, Color.FromArgb(255, 226, 132, 90));
        }

        public OctaneItemViewModel(WorkItem workItem)
        {
            this.workItem = workItem;
        }

        public long ID { get { return workItem.Id; } }
        public string Name { get { return workItem.Name; } }
        public string Phase { get { return workItem.Phase.Name; } }

        public string IconText
        {
            get
            {
                string iconText = "X";
                iconTextMap.TryGetValue(workItem.SubType, out iconText);
                return iconText;
            }
        }

        public Color IconBackgroundColor
        {
            get
            {
                Color bgc = Colors.Red;
                iconBackgroundColorMap.TryGetValue(workItem.SubType, out bgc);
                return bgc;
            }
        }
    }
}
