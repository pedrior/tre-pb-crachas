using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetBarcode;
using SixLabors.ImageSharp.Formats.Png;
using TRE.PB.Cracha.Dialogs;
using TRE.PB.Cracha.Imaging;
using TRE.PB.Cracha.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Badge = TRE.PB.Cracha.Models.Badge;
using Image = SixLabors.ImageSharp.Image;

namespace TRE.PB.Cracha.ViewModels;

public sealed partial class MainViewModel(
    IFileService fileService,
    ISnackbarService snackbarService,
    IDialogHostProvider dialogHostProvider) : ObservableObject
{
    private const int EnrollmentTypingDelayMs = 1000;
    private const double PhotoAspectRatio = 3.2 / 4.0;

    private Guid? editingBadgeId;

    private System.Timers.Timer? enrollmentTypingTimer;

    [ObservableProperty] private string? firstLastName;
    [ObservableProperty] private string? position;
    [ObservableProperty] private string? fullName;
    [ObservableProperty] private string? birthdate;
    [ObservableProperty] private string? bloodType;
    [ObservableProperty] private string? identity;
    [ObservableProperty] private string? identityDate;
    [ObservableProperty] private string? cpf;
    [ObservableProperty] private string? voterId;
    [ObservableProperty] private string? voterZone;
    [ObservableProperty] private string? voterSection;
    [ObservableProperty] private string? enrollment;
    [ObservableProperty] private ImageSource? enrollmentBarcode;
    [ObservableProperty] private ImageSource? photo;
    [ObservableProperty] private bool isPhotoOpen;
    [ObservableProperty] private bool isPhotoLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportAllBadgesCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveAllBadgesCommand))]
    private bool hasBadges;

    [ObservableProperty] private ObservableCollection<Badge> badges = new();

    [RelayCommand]
    private async Task OpenPhotoAsync()
    {
        try
        {
            var file = fileService.OpenFile(
                title: "Selecionar foto",
                filter: "Arquivos de imagem (*.jpg, *.jpeg, *.png, *.bmp, *.pdf)|*.jpg;*.jpeg;*.png;*.bmp;*.pdf");

            if (file is not null)
            {
                LoadPhoto(file);
            }
        }
        catch (Exception e)
        {
            await DialogHelper.ShowDetailedErrorDialogAsync(
                dialogHostProvider.Host,
                "Ocorreu um erro ao abrir a foto selecionada.",
                e.Message,
                e.ToString());
        }
    }

    [RelayCommand]
    private void RemovePhoto()
    {
        Photo = null;
        IsPhotoOpen = false;
    }

    [RelayCommand]
    private async Task Save(UIElement[] elements)
    {
        try
        {
            var coverImage = BitmapHelper.FromUIElement(elements[0]);
            var frontImageFilename = BitmapHelper.SaveToTempPng(BitmapHelper.FromUIElement(elements[1]));
            var backImageFilename = BitmapHelper.SaveToTempPng(BitmapHelper.FromUIElement(elements[2]));

            var badge = new Badge
            {
                FirstLastName = FirstLastName,
                Position = Position,
                FullName = FullName,
                Birthdate = Birthdate,
                BloodType = BloodType,
                Identity = Identity,
                IdentityDate = IdentityDate,
                Cpf = Cpf,
                VoterId = VoterId,
                VoterZone = VoterZone,
                VoterSection = VoterSection,
                Enrollment = Enrollment,
                EnrollmentBarcode = EnrollmentBarcode,
                PhotoImage = Photo,
                FrontImageFilename = frontImageFilename,
                BackImageFilename = backImageFilename,
                CoverImage = coverImage
            };

            if (editingBadgeId.HasValue && Badges.Any(b => b.Id == editingBadgeId.Value))
            {
                var badgeIndex = Badges.IndexOf(Badges.First(b => b.Id == editingBadgeId));
                Badges[badgeIndex] = badge;

                editingBadgeId = null;
            }
            else
            {
                Badges.Add(badge);
                HasBadges = true;
            }

            Clear();
        }
        catch (Exception e)
        {
            await DialogHelper.ShowDetailedErrorDialogAsync(
                dialogHostProvider.Host,
                "Ocorreu um erro ao salvar o crachá.",
                e.Message,
                e.ToString());
        }
    }

    [RelayCommand]
    private void Clear()
    {
        FirstLastName = null;
        Position = null;
        FullName = null;
        Birthdate = null;
        BloodType = null;
        Identity = null;
        IdentityDate = null;
        Cpf = null;
        VoterId = null;
        VoterZone = null;
        VoterSection = null;
        Enrollment = null;
        EnrollmentBarcode = null;
        Photo = null;
        IsPhotoOpen = false;
    }

    [RelayCommand]
    private void EditBadge(Guid badgeId)
    {
        var badge = Badges.FirstOrDefault(b => b.Id == badgeId);
        if (badge is null)
        {
            return;
        }

        FirstLastName = badge.FirstLastName;
        Position = badge.Position;
        FullName = badge.FullName;
        Birthdate = badge.Birthdate;
        BloodType = badge.BloodType;
        Identity = badge.Identity;
        IdentityDate = badge.IdentityDate;
        Cpf = badge.Cpf;
        VoterId = badge.VoterId;
        VoterZone = badge.VoterZone;
        VoterSection = badge.VoterSection;
        Enrollment = badge.Enrollment;
        EnrollmentBarcode = badge.EnrollmentBarcode;
        Photo = badge.PhotoImage;
        IsPhotoOpen = badge.PhotoImage is not null;

        editingBadgeId = badge.Id;
    }

    [RelayCommand]
    private void RemoveBadge(Guid badgeId)
    {
        var badge = Badges.FirstOrDefault(b => b.Id == badgeId);
        if (badge is null)
        {
            return;
        }

        Badges.Remove(badge);
        HasBadges = Badges.Any();
    }

    [RelayCommand(CanExecute = nameof(HasBadges))]
    private void RemoveAllBadges()
    {
        Badges.Clear();
        HasBadges = false;
    }
    
    [RelayCommand]
    private async Task About()
    {
        var dialog = new ContentDialog
        {
            Title = "Sobre o Criador de Crachá do TRE-PB",
            Content = "Esta ferramenta é de uso interno do TRE-PB desenvolvida para facilitar a criação de crachás " +
                      "do TRE-PB. Dúvidas, problemas ou sugestões, entre em contato com a SESOP.",
            CloseButtonText = "Entendi",
            DialogHost = dialogHostProvider.Host,
            DefaultButton = ContentDialogButton.Primary
        };

        await dialog.ShowAsync();
    }

    [RelayCommand(CanExecute = nameof(HasBadges))]
    private async Task ExportAllBadges()
    {
        var relativeDirectory = fileService.OpenFolder("Exportar crachás para...");
        if (relativeDirectory is null)
        {
            return;
        }

        try
        {
            foreach (var badge in Badges)
            {
                var absoluteDirectory = Path.Combine(
                    relativeDirectory,
                    $"CRACHÁ - {badge.FullName ?? shortid.ShortId.Generate()}");
                
                Directory.CreateDirectory(absoluteDirectory);

                File.Copy(badge.FrontImageFilename, Path.Combine(absoluteDirectory, "Frente.png"), overwrite: true);
                File.Copy(badge.BackImageFilename, Path.Combine(absoluteDirectory, "Verso.png"), overwrite: true);
            }

            snackbarService.Show(
                title: "Crachás exportados",
                message: $"Os crachás foram exportados com sucesso para a pasta {relativeDirectory}.",
                appearance: ControlAppearance.Success,
                icon: new SymbolIcon(SymbolRegular.Checkmark20),
                timeout: TimeSpan.FromSeconds(3));
        }
        catch (Exception e)
        {
            await DialogHelper.ShowDetailedErrorDialogAsync(
                dialogHostProvider.Host,
                "Ocorreu um erro ao exportar os crachás.",
                e.Message,
                e.ToString());
        }
    }

    private void LoadPhoto(string filepath)
    {
        IsPhotoLoading = true;

        if (filepath.EndsWith(".pdf"))
        {
            filepath = ConvertPdfToImage(filepath);
        }

        var image = new BitmapImage(new Uri(filepath));

        Photo = BitmapHelper.CropToAspectRatioCentered(image, PhotoAspectRatio);
        IsPhotoOpen = true;
        IsPhotoLoading = false;
    }

    partial void OnEnrollmentChanged(string? value)
    {
        if (enrollmentTypingTimer is not null)
        {
            enrollmentTypingTimer.Stop();
            enrollmentTypingTimer.Dispose();
        }

        enrollmentTypingTimer = new System.Timers.Timer(EnrollmentTypingDelayMs);
        enrollmentTypingTimer.Elapsed += (_, _) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    EnrollmentBarcode = null;
                    return;
                }

                var barcode = new Barcode(value, NetBarcode.Type.Code128, showLabel: false);
                EnrollmentBarcode = ConvertSixLaborsImageToBitmapImage(barcode.GetImage());
            });
        };

        enrollmentTypingTimer.Start();
    }

    private static string ConvertPdfToImage(string filepath)
    {
        using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        var filePath = Path.GetTempFileName();

        PDFtoImage.Conversion.SavePng(filePath, stream, Index.Start);

        return filePath;
    }

    private static BitmapImage ConvertSixLaborsImageToBitmapImage(Image image)
    {
        using var stream = new MemoryStream();

        image.Save(stream, new PngEncoder());
        stream.Seek(0, SeekOrigin.Begin);

        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.StreamSource = stream;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();

        return bitmap;
    }
}