using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TRE.PB.Crachas.Extensions;

public static class UIElementExtensions
{
    public static BitmapImage GetBitmapImage(this UIElement element)
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

    private static BitmapImage EncodeToPngBitmapImage(RenderTargetBitmap bitmapSource)
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
}