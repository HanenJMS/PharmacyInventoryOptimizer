namespace PharmacyInventoryOptimizer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register the route to your scanner page
            Routing.RegisterRoute(nameof(ScannerPage), typeof(ScannerPage));
        }
    }
}
