using MicroFocus.Adm.Octane.Api.Core.Entities;
using System;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    public class FieldInfo
    {
        public string Name { get; private set; }
        public string Title { get; private set; }
        public string EmptyPlaceholder { get; private set; }
        public FieldPosition Position { get; private set; }
        public Func<BaseEntity, object> ContentFunc { get; }

        public FieldInfo(string name, string title, string emptyPlaceholder, FieldPosition position, Func<BaseEntity, object> contentFunc = null)
        {
            Name = name;
            Title = title;
            EmptyPlaceholder = emptyPlaceholder;
            Position = position;
            ContentFunc = contentFunc;
        }
    }
}
