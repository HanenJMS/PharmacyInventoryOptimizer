using ZXing.Net.Maui;

namespace PharmacyInventoryOptimizer;

public partial class ScannerPage : ContentPage
{
	public ScannerPage()
	{
        InitializeComponent();

        // Configure specifically for DataMatrix to boost speed and accuracy
        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.DataMatrix,
            AutoRotate = true,
            Multiple = false
        };

    }
    protected void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        // Grab the first valid barcode detected
        var result = e.Results?.FirstOrDefault();
        if (result is null) return;

        // Pause detection to avoid firing this event 30 times a second
        barcodeReader.IsDetecting = false;

        // The raw GS1 string (e.g., 01003123456789061725123110A1B2C3)
        string rawGs1Data = result.Value;

        // Route back to the main thread for UI updates or navigation
        Dispatcher.DispatchAsync(async () =>
        {
            await DisplayAlert("Barcode Captured", rawGs1Data, "OK");

            // TODO: Pass 'rawGs1Data' into the GS1 Parser to extract the exact expiration date 

            // Resume scanning when ready
            // barcodeReader.IsDetecting = true;
        });
    }
}