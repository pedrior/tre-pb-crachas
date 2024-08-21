using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TRE.PB.Crachas.Extensions;

public static class BitmapImageExtensions
{
    public static void SaveToPng(this BitmapImage image, string filePath)
    {
        var encoder = new PngBitmapEncoder
        {
            Frames = [BitmapFrame.Create(image)]
        };

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        encoder.Save(stream);
    }

    public static string ConvertToBase64String(this BitmapImage image)
    {
        var encoder = new PngBitmapEncoder
        {
            Frames = [BitmapFrame.Create(image)]
        };

        using var stream = new MemoryStream();
        encoder.Save(stream);

        return Convert.ToBase64String(stream.ToArray());
    }

    public static ImageSource Crop(this BitmapImage image, double aspectRatio, double verticalOffset = 0.0)
    {
        var width = image.PixelWidth;
        var height = image.PixelHeight;

        var ratio = (double)width / height;
        var wider = ratio > aspectRatio;

        var newWidth = wider ? height * aspectRatio : width;
        var newHeight = wider ? height : width / aspectRatio;

        var cropX = wider ? (width - newWidth) / 2 : 0;
        var cropY = wider ? 0 : (height - newHeight) / 2 - verticalOffset;

        cropY = Math.Max(0, Math.Min(cropY, height - newHeight));

        return new CroppedBitmap(image, new Int32Rect(
            x: (int)cropX,
            y: (int)cropY,
            width: (int)newWidth,
            height: (int)newHeight));
    }

    public static double CalculateMaxVerticalOffset(this BitmapImage image, double aspectRatio)  
    {
        var width = image.PixelWidth;
        var height = image.PixelHeight;

        var ratio = (double)width / height;
        var wider = ratio > aspectRatio;

        return wider ? 0 : (height - width / aspectRatio) / 2;
    }
}