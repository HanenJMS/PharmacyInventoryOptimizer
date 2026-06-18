using Microsoft.Maui.Devices; // Needed for HapticFeedback
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace PharmacyInventoryOptimizer;

public partial class ScannerPage : ContentPage
{
    public ScannerPage()
    {
        InitializeComponent();

        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.OneDimensional,
            AutoRotate = true,
            Multiple = true,
            DelayBetweenAnalyzingFrames = 150,
            InitialDelayBeforeAnalyzingFrames = 300,
            DelayBetweenContinuousScans = 1000,
            CameraResolutionSelector = availableResolutions =>
              availableResolutions
                .OrderBy(resolution => Math.Abs((resolution.Width * resolution.Height) - (1280 * 720)))
                .ThenBy(resolution => Math.Abs(resolution.Width - 1280) + Math.Abs(resolution.Height - 720))
                .First()
        };

    }

    protected void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var result = e.Results?.FirstOrDefault();
        if (result is null) return;

        // VIBRATE the phone immediately so you know it read something
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);

        barcodeReader.IsDetecting = false;
        string rawGs1Data = result.Value;

        Dispatcher.DispatchAsync(async () =>
        {
            // Update the UI text immediately
            StatusLabel.Text = "Barcode Detected! Processing...";
            StatusLabel.TextColor = Colors.LightGreen;

            Gs1Data parsedData = Gs1Parser.Parse(rawGs1Data);

            if (!parsedData.IsDscsaCompliant)
            {
                await DisplayAlert("DSCSA Warning", "Missing required serialization data.", "Dismiss");

                // Reset UI
                StatusLabel.Text = "Looking for DataMatrix...";
                StatusLabel.TextColor = Colors.White;
                barcodeReader.IsDetecting = true;
                return;
            }

            string formattedMessage = $"NDC: {parsedData.Ndc}\n" +
                                      $"LOT: {parsedData.Lot}\n" +
                                      $"S/N: {parsedData.SerialNumber}\n" +
                                      $"EXP: {parsedData.ExpirationDate?.ToString("MM/dd/yyyy")}";

            await DisplayAlert("Compliant Item Logged", formattedMessage, "OK");

            // Reset UI for the next scan
            StatusLabel.Text = "Looking for DataMatrix...";
            StatusLabel.TextColor = Colors.White;
            // barcodeReader.IsDetecting = true; // Uncomment to loop
        });
    }
}