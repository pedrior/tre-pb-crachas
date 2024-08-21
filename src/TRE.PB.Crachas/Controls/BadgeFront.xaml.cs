using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TRE.PB.Crachas.Extensions;
using Wpf.Ui.Controls;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBlock = Wpf.Ui.Controls.TextBlock;

namespace TRE.PB.Crachas.Controls;

public partial class BadgeFront
{
    private string photoPath = string.Empty;
    private double photoMaxVerticalOffset;
    
    public BadgeFront()
    {
        InitializeComponent();
    }

    public ContentPresenter? ContentPresenter { get; set; }
    
    public event EventHandler<bool>? IsBusyChanged;

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

    public double PhotoVerticalOffset { get; set; }

    public string PhotoFilePath
    {
        get => photoPath;
        set
        {
            photoPath = value;
            if (string.IsNullOrEmpty(value))
            {
                RemovePhoto();
                return;
            }

            SetPhoto(value);
        }
    }

    private double PhotoVerticalOffsetStep => ImgPhoto.ActualHeight * 0.3;
    
    private void BtnAddPhotoOnClick(object sender, RoutedEventArgs e) => SelectPhoto();

    private void ImgCtxMenuDeleteOnClick(object sender, RoutedEventArgs e) => RemovePhoto();

    private void ImgCtxMenuChangeImage(object sender, RoutedEventArgs e) => SelectPhoto();
    
    private void ImgCtxMenuMoveUp(object sender, RoutedEventArgs e)
    {
        if (PhotoVerticalOffset > -photoMaxVerticalOffset)
        {
            PhotoVerticalOffset -= PhotoVerticalOffsetStep;
        }
        
        SetPhoto(photoPath);
    }

    private void ImgCtxMenuMoveDown(object sender, RoutedEventArgs e)
    {
        if (PhotoVerticalOffset < photoMaxVerticalOffset)
        {
            PhotoVerticalOffset += PhotoVerticalOffsetStep;
        }
        
        SetPhoto(photoPath);
    }

    public void Clean()
    {
        TextBoxShortName.Text = string.Empty;
        TextBoxPosition.Text = string.Empty;
        
        PhotoVerticalOffset = 0;

        RemovePhoto();
    }

    private void SelectPhoto()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Arquivos de imagem (*.jpg, *.jpeg, *.png, *.bmp, *.pdf)|*.jpg;*.jpeg;*.png;*.bmp;*.pdf",
            Title = "Selecionar foto"
        };

        if (dialog.ShowDialog() is false)
        {
            return;
        }

        var worker = new BackgroundWorker();
        worker.DoWork += async (_, _) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusyChanged?.Invoke(this, true);

                BtnAddPhoto.Visibility = Visibility.Hidden;
                ImgPhoto.Visibility = Visibility.Hidden;
                PhotoProgressRing.Visibility = Visibility.Visible;
            });

            try
            {
                var filePath = dialog.FileName;

                // TODO: verificar cabeçalho do arquivo para saber se é PDF
                if (filePath.EndsWith(".pdf"))
                {
                    filePath = ConvertPDFToPNGImage(filePath);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PhotoVerticalOffset = 0;
                    SetPhoto(filePath);
                });
            }
            catch (Exception exception)
            {
                await ShowDetailedErrorDialogAsync(
                    "Erro ao carregar a foto",
                    "Ocorreu um erro ao carregar a foto. Por favor, tente novamente.",
                    exception.ToString());
            }
        };

        worker.RunWorkerCompleted += (_, _) =>
        {
            IsBusyChanged?.Invoke(this, false);
            
            ImgPhoto.Visibility = Visibility.Visible;
            PhotoProgressRing.Visibility = Visibility.Hidden;
        };

        worker.RunWorkerAsync();
    }

    private string ConvertPDFToPNGImage(string pdfFilePath)
    {
        using var stream = new FileStream(pdfFilePath, FileMode.Open, FileAccess.Read);
        var filePath = $"{Path.GetTempPath()}\\{Guid.NewGuid()}.png";
        PDFtoImage.Conversion.SavePng(filePath, stream, Index.Start);

        return filePath;
    }

    private void SetPhoto(string fileName)
    {
        var image = new BitmapImage(new Uri(fileName));
        const double aspectRatio = 3.2 / 4.0;

        photoMaxVerticalOffset = image.CalculateMaxVerticalOffset(aspectRatio);
        
        ImgPhoto.Source = image.Crop(aspectRatio, PhotoVerticalOffset);

        ContainerPhoto.Visibility = Visibility.Visible;
        BtnAddPhoto.Visibility = Visibility.Hidden;
        
        photoPath = fileName;
    }

    private void RemovePhoto()
    {
        ImgPhoto.Source = null;

        ContainerPhoto.Visibility = Visibility.Hidden;
        BtnAddPhoto.Visibility = Visibility.Visible;

        photoPath = string.Empty;
    }

    private void ImgPhotoOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (ShowPhotoContainerContextMenu())
        {
            e.Handled = true;
        }
    }

    private bool ShowPhotoContainerContextMenu()
    {
        var menu = ImgPhoto.ContextMenu;
        if (menu is null)
        {
            return false;
        }

        menu.PlacementTarget = ImgPhoto;
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