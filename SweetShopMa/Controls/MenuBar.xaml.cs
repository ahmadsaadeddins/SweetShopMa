using SweetShopMa.ViewModels;

namespace SweetShopMa.Controls;

public partial class MenuBar : ContentView
{
    public MenuBar()
    {
        InitializeComponent();
        
        // Use a small delay/event to ensure Handler and MauiContext are available for DI
        this.Loaded += (s, e) =>
        {
            if (BindingContext == null)
            {
                // Access DI container from the current application handler
                var services = Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;
                if (services != null)
                {
                    BindingContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<MenuBarViewModel>(services);
                }
            }
        };
    }

    private void OnFileMenuClicked(object sender, EventArgs e) => ToggleDropdown(FileDropdown);
    private void OnViewMenuClicked(object sender, EventArgs e) => ToggleDropdown(ViewDropdown);
    private void OnHelpMenuClicked(object sender, EventArgs e) => ToggleDropdown(HelpDropdown);

    private void ToggleDropdown(Frame dropdown)
    {
        // Close all other dropdowns
        FileDropdown.IsVisible = dropdown == FileDropdown ? !FileDropdown.IsVisible : false;
        ViewDropdown.IsVisible = dropdown == ViewDropdown ? !ViewDropdown.IsVisible : false;
        HelpDropdown.IsVisible = dropdown == HelpDropdown ? !HelpDropdown.IsVisible : false;
    }
}
