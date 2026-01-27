using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using SweetShopMa.Services;
using SweetShopMa.ViewModels;
using SweetShopMa.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SweetShopMa.Platforms.Windows;

public class WindowsKeyboardService : IKeyboardService
{
    private readonly IServiceProvider _serviceProvider;
    private Microsoft.UI.Xaml.Window _window;

    public WindowsKeyboardService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize(object window)
    {
        if (window is Microsoft.UI.Xaml.Window win)
        {
            _window = win;
            // Native Window KeyDown event is better because it's global to the window
            _window.Content.KeyDown += OnKeyDown;
        }
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        var isCtrlPressed = global::Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(global::Windows.UI.Core.CoreVirtualKeyStates.Down);
        var isShiftPressed = global::Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(global::Windows.UI.Core.CoreVirtualKeyStates.Down);

        // Map Keys to Commands
        HandleKeyboardShortcut(e.Key, isCtrlPressed, isShiftPressed, e);
    }

    private void HandleKeyboardShortcut(VirtualKey key, bool ctrl, bool shift, KeyRoutedEventArgs e)
    {
        var shopVM = _serviceProvider.GetService<ShopViewModel>();
        var menuBarVM = _serviceProvider.GetService<MenuBarViewModel>();

        // F-Keys (No Modifiers)
        if (!ctrl && !shift)
        {
            switch (key)
            {
                case VirtualKey.F1:
                    if (shopVM != null && shopVM.IsCheckoutEnabled && shopVM.CheckoutCommand.CanExecute(null))
                    {
                        shopVM.CheckoutCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
                case VirtualKey.F2:
                    menuBarVM?.NavigateToDashboardCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case VirtualKey.F4:
                    menuBarVM?.NavigateToInventoryCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case VirtualKey.F5:
                    menuBarVM?.NavigateToAttendanceCommand?.Execute(null);
                    e.Handled = true;
                    break;
                case VirtualKey.F7:
                    menuBarVM?.NavigateToExpensesCommand?.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        // Ctrl + Key
        if (ctrl && !shift)
        {
            switch (key)
            {
                case VirtualKey.N:
                    // Ensure we are on the shop page when starting a new sale
                    _ = Shell.Current.GoToAsync("///shop");
                    
                    if (shopVM != null)
                    {
                        shopVM.QuickSearchText = "";
                        shopVM.QuickQuantityText = "1";
                        _ = shopVM.FocusBarcodeFieldImmediate();
                        e.Handled = true;
                    }
                    else
                    {
                        menuBarVM?.NewSaleCommand?.Execute(null);
                        e.Handled = true;
                    }
                    break;
                case VirtualKey.O:
                    if (shopVM != null && shopVM.OpenDrawerCommand.CanExecute(null))
                    {
                        shopVM.OpenDrawerCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;
                case VirtualKey.L:
                    if (shopVM != null)
                    {
                        shopVM.LogoutCommand?.Execute(null);
                        e.Handled = true;
                    }
                    break;
            }
        }
    }
}
