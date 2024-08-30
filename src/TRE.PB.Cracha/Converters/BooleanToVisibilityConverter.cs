using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TRE.PB.Cracha.Converters;

public sealed class BooleanToVisibilityConverter  : IValueConverter
{
    public Visibility TrueValue { get; set; } = Visibility.Visible;
    
    public Visibility FalseValue { get; set; } = Visibility.Collapsed;
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        value is not bool boolean ? FalseValue : boolean ? TrueValue : FalseValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        value is Visibility visibility && visibility == TrueValue;
}