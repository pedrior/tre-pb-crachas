using TRE.PB.Cracha.Services;
using TRE.PB.Cracha.ViewModels;
using Wpf.Ui;

namespace TRE.PB.Cracha.Views;

public partial class MainView
{
    public MainView(
        MainViewModel viewModel,
        ISnackbarService snackbarService,
        IDialogHostProvider dialogHostProvider)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        dialogHostProvider.SetHost(DialogContentPresenter);
    }
}