using System.Globalization;
using System.Windows.Data;

namespace VolumeMaster.converter
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool original = (bool)value;
            return !original;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool original = (bool)value;
            return !original;
        }
    }
}
