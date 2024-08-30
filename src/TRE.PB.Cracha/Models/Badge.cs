using System.Windows.Media;

namespace TRE.PB.Cracha.Models;

public sealed record Badge
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required string? FirstLastName { get; init; }
    
    public required string? Position { get; init; }
    
    public required string? FullName { get; init; }
    
    public required string? Birthdate { get; init; }
    
    public required string? BloodType { get; init; }
    
    public required string? Identity { get; init; }
    
    public required string? IdentityDate { get; init; }
    
    public required string? Cpf { get; init; }
    
    public required string? VoterId { get; init; }
    
    public required string? VoterZone { get; init; }
    
    public required string? VoterSection { get; init; }
    
    public required string? Enrollment { get; init; }
    
    public required ImageSource? EnrollmentBarcode { get; init; }
    
    public required ImageSource? PhotoImage { get; init; }
    
    public required ImageSource CoverImage { get; init; }
    
    public required string FrontImageFilename { get; init; }
    
    public required string BackImageFilename { get; init; }
}