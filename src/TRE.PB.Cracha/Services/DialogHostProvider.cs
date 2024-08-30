using System.Windows.Controls;

namespace TRE.PB.Cracha.Services;

public sealed class DialogHostProvider : IDialogHostProvider
{
    public ContentPresenter Host { get; private set; } = null!;

    public void SetHost(ContentPresenter host) => Host = host;
}