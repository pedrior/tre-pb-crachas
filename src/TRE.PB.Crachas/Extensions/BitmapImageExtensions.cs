using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TRE.PB.Crachas.Extensions;

public static class BitmapImageExtensions
{
    public static string ToBase64(this BitmapImage bitmap)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var stream = new MemoryStream();
        encoder.Save(stream);

        return Convert.ToBase64String(stream.ToArray());
    }

    public static void SaveAsPng(this BitmapImage bitmap, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var stream = new FileStream(path, FileMode.Create);
        encoder.Save(stream);
    }

    public static double GetMaximumVerticalOffset(this BitmapImage image, double targetAspectRatio)
    {
        var width = image.PixelWidth;
        var height = image.PixelHeight;

        var actualAspectRatio = (double)width / height;
        if (actualAspectRatio > targetAspectRatio)
        {
            return 0;
        }

        return (height - width / targetAspectRatio) / 2;
    }

    public static ImageSource CropToAspectRatio(this BitmapImage image, double targetAspectRatio, double verticalOffset)
    {
        var width = image.PixelWidth;
        var height = image.PixelHeight;

        var actualAspectRatio = (double)width / height;
        if (actualAspectRatio > targetAspectRatio)
        {
            var newWidth = height * targetAspectRatio;
            var crop = (width - newWidth) / 2;
            
            return new CroppedBitmap(image, new Int32Rect((int)crop, 0, (int)newWidth, height));
        }
        else
        {
            var newHeight = width / targetAspectRatio;
            var crop = (height - newHeight) / 2 - verticalOffset;
            
            crop = Math.Max(0, Math.Min(crop, height - newHeight));

            return new CroppedBitmap(image, new Int32Rect(0, (int)crop, width, (int)newHeight));
        }
    }
}