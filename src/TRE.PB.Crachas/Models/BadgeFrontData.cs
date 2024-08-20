namespace TRE.PB.Crachas.Models;

public sealed class BadgeFrontData
{
    public BadgeFrontData(string shortName, string position, string picturePath, double pictureVerticalOffset)
    {
        ShortName = shortName;
        Position = position;
        PicturePath = picturePath;
        PictureVerticalOffset = pictureVerticalOffset;
    }

    public string ShortName { get; }

    public string Position { get; }
    
    public string PicturePath { get; }
    
    public double PictureVerticalOffset { get; }
}