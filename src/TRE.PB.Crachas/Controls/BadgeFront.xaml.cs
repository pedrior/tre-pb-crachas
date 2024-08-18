﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBlock = Wpf.Ui.Controls.TextBlock;

namespace TRE.PB.Crachas.Controls;

public partial class BadgeFront
{
    private string picturePath = string.Empty;

    public BadgeFront()
    {
        InitializeComponent();
    }
    
    public ContentPresenter? ContentPresenter { get; set; }

    public string ShortName
    {
        get => TextBoxShortName.Text;
        set => TextBoxShortName.Text = value;
    }

    public string Position
    {
        get => TextBoxPosition.Text;
        set => TextBoxPosition.Text = value;
    }

    public string PicturePath
    {
        get => picturePath;
        set
        {
            picturePath = value;
            if (string.IsNullOrEmpty(value))
            {
                RemovePhoto();
                return;
            }

            SetPhoto(value);
        }
    }

    private void BtnAddPhotoOnClick(object sender, RoutedEventArgs e) => SelectPhoto();

    private void ImgCtxMenuDeleteOnClick(object sender, RoutedEventArgs e) => RemovePhoto();

    private void ImgCtxMenuChangeImage(object sender, RoutedEventArgs e) => SelectPhoto();

    public void Clean()
    {
        TextBoxShortName.Text = string.Empty;
        TextBoxPosition.Text = string.Empty;

        RemovePhoto();
    }

    private async void SelectPhoto()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Arquivos de imagem (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            Title = "Selecionar foto"
        };

        if (dialog.ShowDialog() is false)
        {
            return;
        }

        try
        {
            SetPhoto(dialog.FileName);
        }
        catch (Exception exception)
        {
            await ShowDetailedErrorDialogAsync(
                "Erro ao carregar a foto",
                "Ocorreu um erro ao carregar a foto. Por favor, tente novamente.",
                exception.ToString());
        }
    }

    private void SetPhoto(string fileName)
    {
        var bitmapImage = new BitmapImage(new Uri(fileName));
        BorderPhoto.Background = new ImageBrush(bitmapImage);

        BorderPhoto.Visibility = Visibility.Visible;
        BtnAddPhoto.Visibility = Visibility.Hidden;

        picturePath = fileName;
    }

    private void RemovePhoto()
    {
        BorderPhoto.Background = null;

        BorderPhoto.Visibility = Visibility.Hidden;
        BtnAddPhoto.Visibility = Visibility.Visible;

        picturePath = string.Empty;
    }

    private void BorderPhotoOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (ShowPhotoContainerContextMenu())
        {
            e.Handled = true;
        }
    }

    private bool ShowPhotoContainerContextMenu()
    {
        var menu = BorderPhoto.ContextMenu;
        if (menu is null)
        {
            return false;
        }

        menu.PlacementTarget = BorderPhoto;
        menu.IsOpen = true;

        return true;
    }

    private async Task ShowDetailedErrorDialogAsync(string title, string text, string details)
    {
        var dialog = new ContentDialog
        {
            Title = title,
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
            },
            CloseButtonText = "Entendi",
            DialogHost = ContentPresenter
        };

        await dialog.ShowAsync();
    }
}