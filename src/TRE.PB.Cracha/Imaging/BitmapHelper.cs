using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TRE.PB.Cracha.Imaging;

public static class BitmapHelper
{
    public static BitmapImage CropToAspectRatioCentered(BitmapImage image, double aspectRatio)
    {
        var optimalVerticalOffset = CalculateOptimalVerticalOffset(image, aspectRatio);

        var width = image.PixelWidth;
        var height = image.PixelHeight;

        var ratio = (double)width / height;
        var isWider = ratio > aspectRatio;

        var newWidth = isWider ? height * aspectRatio : width;
        var newHeight = isWider ? height : width / aspectRatio;

        var cropX = isWider ? (width - newWidth) / 2 : 0;
        var cropY = isWider ? 0 : (height - newHeight) / 2 - optimalVerticalOffset;

        cropY = Math.Max(0, Math.Min(cropY, height - newHeight));

        var cropped = new CroppedBitmap(image, new Int32Rect(
            x: (int)cropX,
            y: (int)cropY,
            width: (int)newWidth,
            height: (int)newHeight));
        
        return EncodeToPngBitmapImage(cropped);
    }

    public static string SaveToTempPng(BitmapImage image)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));

        var filename = Path.GetTempFileName();
        
        using var stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
        encoder.Save(stream);

        return filename;
    }
    public static BitmapImage FromUIElement(UIElement element)
    {
        var dpi = VisualTreeHelper.GetDpi(element);
        var width = element.RenderSize.Width * dpi.DpiScaleX;
        var height = element.RenderSize.Height * dpi.DpiScaleY;

        var bitmapSource = new RenderTargetBitmap(
            (int)width,
            (int)height,
            dpi.PixelsPerInchX,
            dpi.PixelsPerInchY,
            PixelFormats.Pbgra32);

        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            var brush = new BitmapCacheBrush(element)
            {
                BitmapCache = new BitmapCache
                {
                    SnapsToDevicePixels = true
                }
            };

            context.DrawRectangle(brush, null, new Rect(new Point(), element.RenderSize));
        }

        bitmapSource.Render(visual);

        return EncodeToPngBitmapImage(bitmapSource);
    }

    private static BitmapImage EncodeToPngBitmapImage(BitmapSource bitmapSource)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

        using var stream = new MemoryStream();
        encoder.Save(stream);

        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();

        return bitmap;
    }

    private static double CalculateOptimalVerticalOffset(BitmapImage image, double aspectRatio)
    {
        var width = image.PixelWidth;
        var height = image.PixelHeight;

        var ratio = (double)width / height;
        var wider = ratio > aspectRatio;

        return wider ? 0 : (height - width / aspectRatio) / 2;
    }
}