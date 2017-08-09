namespace Hpe.Nga.Octane.VisualStudio
{
    public class FieldInfo
    {
        public string Name { get; private set; }
        public string Title { get; private set; }
        public string EmptyPlaceholder { get; private set; }
        public FieldPosition Position { get; private set; }

        public FieldInfo(string name, string title, string emptyPlaceholder, FieldPosition position)
        {
            Name = name;
            Title = title;
            EmptyPlaceholder = emptyPlaceholder;
            Position = position;
        }
    }
}
