using System.IO;
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
}