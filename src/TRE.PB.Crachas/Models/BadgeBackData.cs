namespace TRE.PB.Crachas.Models;

public sealed class BadgeBackData
{
    public BadgeBackData(
        string name,
        string position,
        string birthdate,
        string bloodType,
        string id,
        string idDate,
        string cpf,
        string voterId,
        string voterZone,
        string voterSection,
        string enrollment,
        bool isEnrollmentShown)
    {
        Name = name;
        Position = position;
        Birthdate = birthdate;
        BloodType = bloodType;
        Id = id;
        IdDate = idDate;
        Cpf = cpf;
        VoterId = voterId;
        VoterZone = voterZone;
        VoterSection = voterSection;
        Enrollment = enrollment;
        IsEnrollmentShown = isEnrollmentShown;
    }

    public string Name { get; }

    public string Position { get; }

    public string Birthdate { get; }

    public string BloodType { get; }

    public string Id { get; }

    public string IdDate { get; }

    public string Cpf { get; }

    public string VoterId { get; }

    public string VoterZone { get; }

    public string VoterSection { get; }

    public string Enrollment { get; }
    
    public bool IsEnrollmentShown { get; }
}