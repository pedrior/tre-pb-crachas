using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TRE.PB.Cracha.Converters;

public sealed class BadgeValueConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => 
        values.Cast<UIElement>().ToArray();

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}