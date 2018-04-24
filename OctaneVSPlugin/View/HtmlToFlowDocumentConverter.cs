using HTMLConverter;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    public class HtmlToFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                return ConvertHtmlToFlowDocument(value.ToString());
            }
            catch
            {
                try
                {
                    // if there was an issue trying to convert the original html code
                    // we show the user an error message
                    return ConvertHtmlToFlowDocument("<html><body><p><span style=\"color:#e74c3c;\">Unable to obtain information</span></p></body></html>");
                }
                catch
                {
                    // if we really can't convert to FlowDocument, don't show anything
                    return null;
                }
            }
        }

        private FlowDocument ConvertHtmlToFlowDocument(string html)
        {
            var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, false);

            var flowDocument = new FlowDocument();
            using (var stream = new MemoryStream((new UTF8Encoding()).GetBytes(xaml)))
            {
                var text = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                text.Load(stream, DataFormats.Xaml);
            }

            SubscribeToAllHyperlinks(flowDocument);

            flowDocument.FontSize = SystemFonts.IconFontSize;
            flowDocument.FontWeight = SystemFonts.MessageFontWeight;
            flowDocument.FontFamily = SystemFonts.CaptionFontFamily;

            return flowDocument;
        }

        private void SubscribeToAllHyperlinks(FlowDocument flowDocument)
        {
            var hyperlinks = GetVisuals(flowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
                link.RequestNavigate += LinkRequestNavigate;
        }

        private static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var descendants in GetVisuals(child))
                    yield return descendants;
            }
        }

        private void LinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                var hyperLink = (Hyperlink)e.OriginalSource;
                string uri;
                if (e.Uri.IsAbsoluteUri)
                {
                    uri = e.Uri.AbsoluteUri + "#" + hyperLink.TargetName;
                }
                else
                {
                    uri = OctaneConfiguration.Url + hyperLink.NavigateUri;
                }

                Process.Start(new ProcessStartInfo(uri));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            e.Handled = true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
