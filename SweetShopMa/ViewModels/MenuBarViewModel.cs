using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SweetShopMa.Services;
using SweetShopMa.Views;

namespace SweetShopMa.ViewModels;

public class MenuBarViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;
    private readonly LocalizationService _localizationService;
    private readonly IServiceProvider _serviceProvider;

    public MenuBarViewModel(AuthService authService, LocalizationService localizationService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _localizationService = localizationService;
        _serviceProvider = serviceProvider;

        _authService.OnUserChanged += _ => OnPropertyChanged(nameof(CanAccessAdmin));

        // File Menu
        NewSaleCommand = new Command(async () => await NavigateToAsync("shop"));
        LogoutCommand = new Command(async () => await ExecuteOnShopViewModel(vm => vm.LogoutCommand.Execute(null)));
        OpenDrawerCommand = new Command(async () => await ExecuteOnShopViewModel(vm => vm.OpenDrawerCommand.Execute(null)));
        ExitCommand = new Command(() => Application.Current?.Quit());

        // Edit Menu
        AddProductCommand = new Command(async () => await ExecuteOnAdminViewModel(vm => vm.AddProductCommand.Execute(null)));
        
        // View Menu
        NavigateToDashboardCommand = new Command(async () => await NavigateToAsync("admin"));
        NavigateToInventoryCommand = new Command(async () => await NavigateToAsync("products")); // Assuming route exists or needs adding
        NavigateToAttendanceCommand = new Command(async () => await NavigateToAsync("attendance"));
        NavigateToUsersCommand = new Command(async () => await NavigateToAsync("users"));
        NavigateToExpensesCommand = new Command(async () => await NavigateToAsync("expenses"));
        
        // Help Menu
        ShowAboutCommand = new Command(async () => 
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("About", "Sweet Shop POS v1.0", "OK");
        });
    }

    public bool CanAccessAdmin => _authService.CanManageUsers || _authService.CanManageStock;

    // File Commands
    public ICommand NewSaleCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand OpenDrawerCommand { get; }
    public ICommand ExitCommand { get; }

    // Edit Commands
    public ICommand AddProductCommand { get; }

    // View Commands
    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToInventoryCommand { get; }
    public ICommand NavigateToAttendanceCommand { get; }
    public ICommand NavigateToUsersCommand { get; }
    public ICommand NavigateToExpensesCommand { get; }

    // Help Commands
    public ICommand ShowAboutCommand { get; }

    private async Task NavigateToAsync(string route)
    {
        // Shell elements (defined in AppShell.xaml) require absolute routing to switch sections
        if (route == "shop" || route == "login")
        {
            await Shell.Current.GoToAsync($"///{route}");
        }
        else
        {
            // Global routes (registered via Routing.RegisterRoute) require relative routing
            // Prefixing with / as recommended by the MAUI Exception message
            await Shell.Current.GoToAsync($"/{route}");
        }
    }

    private async Task ExecuteOnShopViewModel(Action<ShopViewModel> action)
    {
        var shopVM = _serviceProvider.GetService<ShopViewModel>();
        if (shopVM != null) action(shopVM);
        await Task.CompletedTask;
    }

    private async Task ExecuteOnAdminViewModel(Action<AdminViewModel> action)
    {
        var adminVM = _serviceProvider.GetService<AdminViewModel>();
        if (adminVM != null) action(adminVM);
        await Task.CompletedTask;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
