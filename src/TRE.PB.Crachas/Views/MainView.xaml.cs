using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TRE.PB.Crachas.Controls;
using TRE.PB.Crachas.Models;
using TRE.PB.Crachas.Extensions;
using Wpf.Ui;
using Wpf.Ui.Controls;
using TextBlock = Wpf.Ui.Controls.TextBlock;

namespace TRE.PB.Crachas.Views;

public partial class MainView
{
    private string? badgeBeingEditedUid;
    private readonly ISnackbarService snackbarService;

    public MainView()
    {
        InitializeComponent();
        snackbarService = new SnackbarService();

        SnackbarPresenter.Loaded += (_, _) =>
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        BadgeFront.ContentPresenter = AppDialogContentPresenter;
        
        BadgeFront.IsBusyChanged += (_, isBusy) =>
        {
            if (isBusy)
            {
                BadgeFront.IsEnabled = false;
                ContainerActions.IsEnabled = false;
            }
            else
            {
                BadgeFront.IsEnabled = true;
                ContainerActions.IsEnabled = true;
            }
        };
    }

    private bool IsInEditing => badgeBeingEditedUid is not null;

    private bool AnyBadgeCreated => StkPnlBadgeList.Children.Count is not 0;

    private void BtnClean_OnClick(object sender, RoutedEventArgs e) => ResetBadgeEditor();

    private void BtnSave_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsInEditing)
        {
            CreateNewBadge();
            return;
        }

        var badgeBeingEdited = StkPnlBadgeList.Children
            .OfType<BadgeListItem>()
            .FirstOrDefault(x => x.Uid == badgeBeingEditedUid);

        // O usuário excluiu o crachá logo após clicar em editar
        if (badgeBeingEdited is null)
        {
            CreateNewBadge();
            return;
        }

        badgeBeingEdited.Tag = GetEditorBadgeData();
        badgeBeingEdited.ImageSource = StkPnlFullBadge.GetBitmapImage();

        ResetBadgeEditor();
    }

    private async void BtnRemoveAll_OnClick(object sender, RoutedEventArgs e)
    {
        if (!AnyBadgeCreated)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Limpar crachás",
            Content = "Tem certeza que deseja remover todos os crachás?",
            CloseButtonText = "Não",
            PrimaryButtonText = "Sim",
            PrimaryButtonAppearance = ControlAppearance.Danger,
            DialogHost = AppDialogContentPresenter
        };

        var result = await dialog.ShowAsync();
        if (result is not ContentDialogResult.Primary)
        {
            return;
        }

        RemoveAllBadges();
        ResetBadgeEditor();
        UpdateControls();
    }

    private async void BtnExport_OnClick(object sender, RoutedEventArgs e)
    {
        var badges = StkPnlBadgeList.Children
            .OfType<BadgeListItem>()
            .Select(x => (BadgeData)x.Tag)
            .ToList();

        if (badges.Count is 0)
        {
            return;
        }

        var dialog = new OpenFolderDialog
        {
            Title = "Selecione a pasta de destino",
            DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() is not true)
        {
            return;
        }

        var success = await SaveBadgesAsync(badges, directory: dialog.FolderName);
        if (success)
        {
            snackbarService.Show(
                title: "Crachás exportados",
                message: $"Os crachás foram exportados com sucesso para a pasta {dialog.FolderName}.",
                appearance: ControlAppearance.Success,
                icon: new SymbolIcon(SymbolRegular.Checkmark20),
                timeout: TimeSpan.FromSeconds(3));

            Process.Start("explorer.exe", dialog.FolderName);
            return;
        }

        var message = "Ocorreu um erro ao exportar os crachás. Verifique a pasta de destino e se você possui " +
                      "permissão para gravar arquivos nela.";

        if (badges.Count > 1)
        {
            message += " Ainda assim, alguns crachás podem ter sido exportados com sucesso.";
        }

        snackbarService.Show(
            title: "Erro ao exportar crachás",
            message: message,
            appearance: ControlAppearance.Danger,
            icon: new SymbolIcon(SymbolRegular.ErrorCircle20),
            timeout: TimeSpan.FromSeconds(5));
    }

    private async void BtnAbout_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Sobre o Criador de Crachá do TRE-PB",
            Content = "Esta ferramenta é de uso interno do TRE-PB e foi desenvolvida por Pedro Júnior (SESOP) para " +
                      "facilitar a criação de crachás do TRE-PB pelos setores responsáveis. Dúvidas, problemas ou " +
                      "sugestões, entre em contato com a SESOP no ramal 1346 ou 1343.",
            CloseButtonText = "Entendi",
            DialogHost = AppDialogContentPresenter,
            DefaultButton = ContentDialogButton.Primary
        };

        await dialog.ShowAsync();
    }

    private void BadgeListItem_OnRemoveClick(object sender, RoutedEventArgs e)
    {
        if (sender is not BadgeListItem badgeListItem)
        {
            return;
        }

        RemoveBadge(badgeListItem);
        UpdateControls();
    }

    private void BadgeListItem_OnEditClick(object sender, RoutedEventArgs e)
    {
        var badgeListItem = sender as BadgeListItem;
        if (badgeListItem?.Tag is not BadgeData badgeInfo)
        {
            return;
        }

        badgeBeingEditedUid = badgeListItem.Uid;

        SetEditorBadgeData(badgeInfo);
    }

    private void CreateNewBadge()
    {
        var badgeListItem = new BadgeListItem
        {
            Tag = GetEditorBadgeData(),
            Uid = Guid.NewGuid().ToString(),
            ImageSource = StkPnlFullBadge.GetBitmapImage()
        };

        badgeListItem.EditClick += BadgeListItem_OnEditClick;
        badgeListItem.RemoveClick += BadgeListItem_OnRemoveClick;

        StkPnlBadgeList.Children.Add(badgeListItem);

        ResetBadgeEditor();
        UpdateControls();
    }

    private void RemoveBadge(BadgeListItem badgeListItem)
    {
        badgeListItem.EditClick -= BadgeListItem_OnEditClick;
        badgeListItem.RemoveClick -= BadgeListItem_OnRemoveClick;

        StkPnlBadgeList.Children.Remove(badgeListItem);
    }

    private void RemoveAllBadges()
    {
        var badgeListItems = StkPnlBadgeList.Children
            .OfType<BadgeListItem>()
            .ToList();

        foreach (var badgeListItem in badgeListItems)
        {
            RemoveBadge(badgeListItem);
        }
    }

    private void ResetBadgeEditor()
    {
        BadgeFront.Clean();
        BadgeBack.Clean();

        badgeBeingEditedUid = null;
    }

    private void UpdateControls()
    {
        if (AnyBadgeCreated)
        {
            TextBlockBadgeListEmpty.Visibility = Visibility.Collapsed;
            ButtonClearBadgeList.IsEnabled = true;
            ButtonExport.IsEnabled = true;
        }
        else
        {
            TextBlockBadgeListEmpty.Visibility = Visibility.Visible;
            ButtonClearBadgeList.IsEnabled = false;
            ButtonExport.IsEnabled = false;
        }
    }

    private async Task<bool> SaveBadgesAsync(IEnumerable<BadgeData> badges, string directory)
    {
        try
        {
            foreach (var badge in badges)
            {
                SaveBadge(badge, directory);
            }
        }
        catch (Exception exception)
        {
            await ShowDetailedErrorDialogAsync(
                title: "Erro ao exportar crachás",
                text: "Ocorreu um erro ao exportar os crachás. Verifique se a pasta de destino está correta e se " +
                      "você possui permissão para gravar arquivos nela.",
                details: exception.ToString());

            return false;
        }

        return true;
    }

    private static void SaveBadge(BadgeData badge, string directory)
    {
        var path = CreateFolderForBadge(directory, badge);
        var front = ConvertBase64StringToBitmapImage(badge.FrontImageBase64);
        var back = ConvertBase64StringToBitmapImage(badge.BackImageBase64);

        front.SaveAsPng(Path.Combine(path, "FRENTE.png"));
        back.SaveAsPng(Path.Combine(path, "VERSO.png"));
    }

    private static BitmapImage ConvertBase64StringToBitmapImage(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.StreamSource = new MemoryStream(bytes);
        bitmap.EndInit();

        return bitmap;
    }

    private static string CreateFolderForBadge(string directory, BadgeData badge)
    {
        var name = string.IsNullOrWhiteSpace(badge.Back.Name)
            ? "SEM NOME"
            : badge.Back.Name;

        var path = Path.Combine(directory, $"CRACHÁ - {name}");

        var i = 1;
        while (Path.Exists(path))
        {
            path = Path.Combine(directory, $"CRACHÁ - {name} ({i++})");
        }

        Directory.CreateDirectory(path);

        return path;
    }

    private BadgeData GetEditorBadgeData()
    {
        var badgeFrontData = new BadgeFrontData(
            shortName: BadgeFront.ShortName,
            position: BadgeFront.Position,
            picturePath: BadgeFront.PicturePath);

        var badgeBackData = new BadgeBackData(
            name: BadgeBack.FullName,
            position: BadgeBack.Position,
            birthdate: BadgeBack.Birthdate,
            bloodType: BadgeBack.BloodType,
            id: BadgeBack.Id,
            idDate: BadgeBack.IdDate,
            cpf: BadgeBack.Cpf,
            voterId: BadgeBack.VoterId,
            voterZone: BadgeBack.VoterZone,
            voterSection: BadgeBack.VoterSection,
            enrollment: BadgeBack.Enrollment,
            isEnrollmentShown: BadgeBack.IsEnrollmentShown);

        return new BadgeData(
            front: badgeFrontData,
            back: badgeBackData,
            frontImageBase64: BadgeFront.GetBitmapImage()
                .ToBase64(),
            backImageBase64: BadgeBack.GetBitmapImage()
                .ToBase64());
    }

    private void SetEditorBadgeData(BadgeData data)
    {
        BadgeFront.ShortName = data.Front.ShortName;
        BadgeFront.Position = data.Front.Position;
        BadgeFront.PicturePath = data.Front.PicturePath;

        BadgeBack.FullName = data.Back.Name;
        BadgeBack.Position = data.Back.Position;
        BadgeBack.Birthdate = data.Back.Birthdate;
        BadgeBack.BloodType = data.Back.BloodType;
        BadgeBack.Id = data.Back.Id;
        BadgeBack.IdDate = data.Back.IdDate;
        BadgeBack.Cpf = data.Back.Cpf;
        BadgeBack.VoterId = data.Back.VoterId;
        BadgeBack.VoterZone = data.Back.VoterZone;
        BadgeBack.VoterSection = data.Back.VoterSection;
        BadgeBack.Enrollment = data.Back.Enrollment;
        BadgeBack.IsEnrollmentShown = data.Back.IsEnrollmentShown;
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
            DialogHost = AppDialogContentPresenter
        };

        await dialog.ShowAsync();
    }
}