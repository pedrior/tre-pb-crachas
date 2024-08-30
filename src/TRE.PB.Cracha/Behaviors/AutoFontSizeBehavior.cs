using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace TRE.PB.Cracha.Behaviors;

public static class AutoFontSizeBehavior
{
    public static readonly DependencyProperty EnableAutoSizeProperty =
        DependencyProperty.RegisterAttached("EnableAutoSize", typeof(bool), typeof(AutoFontSizeBehavior),
            new UIPropertyMetadata(false, OnEnableAutoSizeChanged));

    public static bool GetEnableAutoSize(DependencyObject obj) => (bool)obj.GetValue(EnableAutoSizeProperty);

    public static void SetEnableAutoSize(DependencyObject obj, bool value) =>
        obj.SetValue(EnableAutoSizeProperty, value);

    private static void OnEnableAutoSizeChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not TextBox textBlock)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            textBlock.Tag = textBlock.FontSize;
            textBlock.TextChanged += OnTextChanged;
        }
        else
        {
            textBlock.FontSize = (double)textBlock.Tag;
            textBlock.TextChanged -= OnTextChanged;
        }
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBlock)
        {
            return;
        }

        var initialFontSize = (double)textBlock.Tag;
        var newFontSize = CalculateFontSize(
            textBlock.Text,
            textBlock.FontSize,
            initialFontSize,
            textBlock.FontFamily,
            textBlock.FontWeight,
            textBlock.FontStyle,
            textBlock.FontStretch,
            textBlock.ActualWidth,
            textBlock.Margin.Left + textBlock.Margin.Right);

        textBlock.FontSize = newFontSize;
    }

    private static double CalculateFontSize(
        string text,
        double fontSize,
        double initialFontSize,
        FontFamily fontFamily,
        FontWeight fontWeight,
        FontStyle fontStyle,
        FontStretch fontStretch,
        double controlWidth,
        double horizontalMargin)
    {
        var textWidth = MeasureTextWidth(text, fontFamily, fontWeight, fontStyle, fontStretch, fontSize);
        
        while (textWidth > controlWidth - horizontalMargin && fontSize > 0)
        {
            fontSize -= 0.05;
            textWidth = MeasureTextWidth(text, fontFamily, fontWeight, fontStyle, fontStretch, fontSize);
        }
        
        while (textWidth < controlWidth - horizontalMargin && fontSize <= initialFontSize)
        {
            fontSize += 0.05;
            textWidth = MeasureTextWidth(text, fontFamily, fontWeight, fontStyle, fontStretch, fontSize);
        }
        
        return fontSize;
        
        // var textWidth = MeasureTextWidth(text, fontFamily, fontWeight, fontStyle, fontStretch, fontSize);
        //
        // while (textWidth > controlWidth && fontSize > 0)
        // {
        //     fontSize--;
        //     textWidth = MeasureTextWidth(text, fontFamily, fontWeight, fontStyle, fontStretch, fontSize);
        // }
        //
        // while (textWidth < controlWidth && fontSize <= initialFontSize)
        // {
        //     fontSize++;
        //     textWidth = MeasureTextWidth(text, fontFamily, fontWeight, fontStyle, fontStretch, fontSize);
        // }
        //
        // return fontSize;
    }

    private static double MeasureTextWidth(
        string text,
        FontFamily fontFamily,
        FontWeight fontWeight,
        FontStyle fontStyle,
        FontStretch fontStretch,
        double fontSize)
    {
        var formattedText = new FormattedText(
            textToFormat: text,
            culture: CultureInfo.CurrentUICulture,
            flowDirection: FlowDirection.LeftToRight,
            typeface: new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
            emSize: fontSize,
            foreground: Brushes.Black,
            pixelsPerDip: 1.0);

        return formattedText.Width;
    }
}