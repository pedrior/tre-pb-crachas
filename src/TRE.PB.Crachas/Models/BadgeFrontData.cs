namespace TRE.PB.Crachas.Models;

public sealed class BadgeFrontData
{
    public BadgeFrontData(string shortName, string position, string picturePath)
    {
        ShortName = shortName;
        Position = position;
        PicturePath = picturePath;
    }

    public string ShortName { get; }

    public string Position { get; }
    
    public string PicturePath { get; }
}