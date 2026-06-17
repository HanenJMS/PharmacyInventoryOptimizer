namespace PharmacyInventoryOptimizer
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnScanButtonClicked(object sender, EventArgs e)
        {
            // Navigate to the ScannerPage using its registered route name
            await Shell.Current.GoToAsync(nameof(ScannerPage));
        }
    }
}
