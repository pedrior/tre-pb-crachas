using System.Windows.Controls;

namespace TRE.PB.Cracha.Services;

public interface IDialogHostProvider
{
    ContentPresenter Host { get; }
    
    void SetHost(ContentPresenter host);
}