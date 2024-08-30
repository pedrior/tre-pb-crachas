using Microsoft.Win32;

namespace TRE.PB.Cracha.Services;

public sealed class FileService : IFileService
{
    public string? OpenFile(string title, string? filter = null)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter ?? "Todos os arquivos (*.*)|*.*",
            Title = title 
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? OpenFolder(string title)
    {
        var dialog = new OpenFolderDialog()
        {
            Title = title
        };
        
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }
}