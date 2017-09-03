using System;
using System.Globalization;
using System.Windows.Data;

namespace Hpe.Nga.Octane.VisualStudio
{
    /// <summary>
    /// Value converter which convert int value to check if they are greater than another value (0 by default).
    /// </summary>
    public class GreaterThanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int compareTo = 0;
            if (parameter is int)
            {
                compareTo = (int)parameter;
            }

            int intValue = System.Convert.ToInt32(value);
            return intValue > compareTo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
