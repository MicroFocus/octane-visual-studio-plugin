using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Info about the icon label and color.
    /// </summary>
    public class ItemIconInfo
    {
        public string ShortLabel { get; private set; }
        public Color LabelColor { get; private set; }

        public ItemIconInfo(string shortLabel, Color labelColor)
        {
            ShortLabel = shortLabel;
            LabelColor = labelColor;
        }
    }
}
