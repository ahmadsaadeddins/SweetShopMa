using SweetShopMa.Services;
using SweetShopMa.Views;

namespace SweetShopMa
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for pages used in MenuBar
            Routing.RegisterRoute("admin", typeof(AdminPage));
            Routing.RegisterRoute("products", typeof(ProductsPage));
            Routing.RegisterRoute("attendance", typeof(AttendancePage));
            Routing.RegisterRoute("users", typeof(UsersPage));
            Routing.RegisterRoute("expenses", typeof(ExpensesPage));
        }
    }
}
