using System.Windows;
using System.Windows.Media;

namespace TRE.PB.Crachas.Controls;

public partial class BadgeListItem
{
    public event RoutedEventHandler? RemoveClick;
    public event RoutedEventHandler? EditClick;

    public BadgeListItem()
    {
        InitializeComponent();
    }

    private void ButtonRemoveOnClick(object sender, RoutedEventArgs e) => RemoveClick?.Invoke(this, e);

    private void ButtonEditOnClick(object sender, RoutedEventArgs e) => EditClick?.Invoke(this, e);

    public ImageSource? ImageSource
    {
        get => Image.Source;
        set => Image.Source = value;
    }
}