using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    ///  Service for adding a IsTextTrimmed property to TextBlocks
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TextBlockService
    {
        static TextBlockService()
        {
            // Register for the SizeChanged event on all TextBlocks, even if the event was handled.
            EventManager.RegisterClassHandler(typeof(TextBlock),
                FrameworkElement.SizeChangedEvent,
                new SizeChangedEventHandler(OnTextBlockSizeChanged), true);
        }

        public static readonly DependencyPropertyKey IsTextTrimmedKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsTextTrimmed",
                typeof(bool),
                typeof(TextBlockService),
                new PropertyMetadata(false)
            );

        public static readonly DependencyProperty IsTextTrimmedProperty =
            IsTextTrimmedKey.DependencyProperty;

        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static Boolean GetIsTextTrimmed(TextBlock target)
        {
            return (Boolean)target.GetValue(IsTextTrimmedProperty);
        }

        public static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            if (textBlock == null)
            {
                return;
            }
            textBlock.SetValue(IsTextTrimmedKey, CalculateIsTextTrimmed(textBlock));
        }

        private static bool CalculateIsTextTrimmed(TextBlock textBlock)
        {
            double width = textBlock.ActualWidth;
            if (textBlock.TextTrimming == TextTrimming.None)
            {
                return false;
            }
            if (textBlock.TextWrapping != TextWrapping.NoWrap)
            {
                return false;
            }
            textBlock.Measure(new Size(double.MaxValue, double.MaxValue));

            if (double.IsInfinity(textBlock.MaxWidth))
                return width < textBlock.DesiredSize.Width;
            else
            {
                var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
                // FormattedText is used to measure the whole width of the text held up by this container.
                var formmatedText = new FormattedText(textBlock.Text, Thread.CurrentThread.CurrentCulture,
                    textBlock.FlowDirection, typeface, textBlock.FontSize, textBlock.Foreground);

                return formmatedText.Width > width;
            }
        }
    }
}
