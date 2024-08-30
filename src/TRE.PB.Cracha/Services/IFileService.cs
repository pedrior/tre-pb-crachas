namespace TRE.PB.Cracha.Services;

public interface IFileService
{
    string? OpenFile(string title, string? filter = null);
    
    string? OpenFolder(string title);
}