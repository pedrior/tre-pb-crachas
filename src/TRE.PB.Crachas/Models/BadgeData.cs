namespace TRE.PB.Crachas.Models;

public sealed class BadgeData
{
    public BadgeData(BadgeFrontData front, BadgeBackData back, string frontImageBase64, string backImageBase64)
    {
        Front = front;
        Back = back;
        FrontImageBase64 = frontImageBase64;
        BackImageBase64 = backImageBase64;
    }

    public BadgeFrontData Front { get; }

    public BadgeBackData Back { get; }

    public string FrontImageBase64 { get; }

    public string BackImageBase64 { get; }
}