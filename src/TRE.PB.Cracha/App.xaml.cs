using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TRE.PB.Cracha.Services;
using TRE.PB.Cracha.ViewModels;
using TRE.PB.Cracha.Views;
using Wpf.Ui;

namespace TRE.PB.Cracha;

public partial class App
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var exception = (Exception)e.ExceptionObject;
            MessageBox.Show(
                messageBoxText: exception.ToString(),
                caption: exception.Message,
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error);
        };

        Services = ConfigureServices();

        InitializeComponent();
    }
    
    public IServiceProvider Services { get; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Services.GetRequiredService<MainView>()
            .Show();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDialogHostProvider, DialogHostProvider>();
        services.AddSingleton<ISnackbarService, SnackbarService>();
        
        services.AddTransient<IFileService, FileService>();

        services.AddTransient<MainViewModel>();

        services.AddSingleton<MainView>();

        return services.BuildServiceProvider();
    }
}