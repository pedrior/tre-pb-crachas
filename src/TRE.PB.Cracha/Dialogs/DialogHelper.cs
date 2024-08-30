using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using TextBlock = Wpf.Ui.Controls.TextBlock;

namespace TRE.PB.Cracha.Dialogs;

public static class DialogHelper
{
    public static async Task ShowDetailedErrorDialogAsync(
        ContentPresenter host,
        string title,
        string text,
        string details)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            DialogHost = host,
            CloseButtonText = "Entendi",
            Content = new Grid
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = text,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new Expander
                    {
                        Header = "Detalhes do erro",
                        Margin = new Thickness(0, 24, 0, 0),
                        Content = new TextBlock
                        {
                            Text = details,
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                }
            }
        };

        await dialog.ShowAsync();
    }
}