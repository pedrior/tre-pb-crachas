using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NetBarcode;
using SixLabors.ImageSharp.Formats.Png;
using Image = SixLabors.ImageSharp.Image;

namespace TRE.PB.Crachas.Controls;

public partial class BadgeBack
{
    private const int EnrollmentTypingDelayMs = 1000;

    private bool isEnrollmentShown = true;

    private System.Timers.Timer? enrollmentTypingTimer;

    public bool IsEnrollmentShown
    {
        get => isEnrollmentShown;
        set
        {
            isEnrollmentShown = value;

            if (isEnrollmentShown)
            {
                TextBoxEnrollment.IsReadOnly = false;
                TextBoxEnrollment.PlaceholderText = "MATRÍCULA";
            }
            else
            {
                TextBoxEnrollment.IsReadOnly = true;
                TextBoxEnrollment.Text = string.Empty;
                TextBoxEnrollment.PlaceholderText = string.Empty;
                ImgEnrollmentBarCode.Source = null;
            }
        }
    }
    
    public BadgeBack()
    {
        InitializeComponent();
    }

    public string FullName
    {
        get => TextBoxFullName.Text;
        set => TextBoxFullName.Text = value;
    }

    public string Position
    {
        get => TextBoxPosition.Text;
        set => TextBoxPosition.Text = value;
    }

    public string Birthdate
    {
        get => TextBoxBirthate.Text;
        set => TextBoxBirthate.Text = value;
    }

    public string BloodType
    {
        get => TextBoxBloodType.Text;
        set => TextBoxBloodType.Text = value;
    }

    public string Id
    {
        get => TextBoxId.Text;
        set => TextBoxId.Text = value;
    }

    public string IdDate
    {
        get => TextBoxIdDate.Text;
        set => TextBoxIdDate.Text = value;
    }

    public string Cpf
    {
        get => TextBoxCpf.Text;
        set => TextBoxCpf.Text = value;
    }

    public string VoterId
    {
        get => TextBoxVoterId.Text;
        set => TextBoxVoterId.Text = value;
    }

    public string VoterZone
    {
        get => TextBoxVoterZone.Text;
        set => TextBoxVoterZone.Text = value;
    }

    public string VoterSection
    {
        get => TextBoxVoterSection.Text;
        set => TextBoxVoterSection.Text = value;
    }

    public string Enrollment
    {
        get => TextBoxEnrollment.Text;
        set => TextBoxEnrollment.Text = value;
    }

    public void Clean()
    {
        FullName = string.Empty;
        Position = string.Empty;
        Birthdate = string.Empty;
        BloodType = string.Empty;
        Id = string.Empty;
        IdDate = string.Empty;
        Cpf = string.Empty;
        VoterId = string.Empty;
        VoterZone = string.Empty;
        VoterSection = string.Empty;
        Enrollment = string.Empty;
        ImgEnrollmentBarCode.Source = null;
    }

    private void TxtBxEnrollmentOnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (enrollmentTypingTimer is not null)
        {
            enrollmentTypingTimer.Stop();
            enrollmentTypingTimer.Dispose();
        }

        enrollmentTypingTimer = new System.Timers.Timer(EnrollmentTypingDelayMs);
        enrollmentTypingTimer.Elapsed += (_, _) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (string.IsNullOrWhiteSpace(TextBoxEnrollment.Text))
                {
                    ImgEnrollmentBarCode.Source = null;
                    return;
                }

                GenerateEnrollmentBarcode(TextBoxEnrollment.Text);
            });
        };

        enrollmentTypingTimer.Start();
    }

    private void GenerateEnrollmentBarcode(string enrollment)
    {
        var barcode = new Barcode(enrollment, NetBarcode.Type.Code128, showLabel: false);
        ImgEnrollmentBarCode.Source = ConvertSixLaborsImageToBitmapImage(barcode.GetImage());
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

    private void EnrollmentContextMenuShowHide_OnClick(object sender, RoutedEventArgs e)
    {
        if (isEnrollmentShown)
        {
            ImgEnrollmentBarCode.Source = null;

            TextBoxEnrollment.IsReadOnly = true;
            TextBoxEnrollment.Text = string.Empty;
            
            TextBoxEnrollment.PlaceholderText = string.Empty;
        }
        else
        {
            TextBoxEnrollment.IsReadOnly = false;
            TextBoxEnrollment.PlaceholderText = "MATRÍCULA";
        }

        isEnrollmentShown = !isEnrollmentShown;
    }
}