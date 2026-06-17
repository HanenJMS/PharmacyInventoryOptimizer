using ZXing.Net.Maui;
using System.Diagnostics; // For Debug logging

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
        var result = e.Results?.FirstOrDefault();
        if (result is null) return;

        barcodeReader.IsDetecting = false;
        string rawGs1Data = result.Value;

        Dispatcher.DispatchAsync(async () =>
        {
            Gs1Data parsedData = Gs1Parser.Parse(rawGs1Data);

            // --- ENFORCE DSCSA COMPLIANCE ---
            if (!parsedData.IsDscsaCompliant)
            {
                // Reject the scan, alert the user, and re-enable the scanner
                await DisplayAlert(
                    "DSCSA Warning",
                    "This barcode is missing required serialization data (NDC, Lot, SN, or EXP) and is non-compliant.",
                    "Dismiss"
                );

                barcodeReader.IsDetecting = true;
                return;
            }

            // Output the extracted NDC, not the raw GTIN
            string formattedMessage = $"NDC: {parsedData.Ndc}\n" +
                                      $"LOT: {parsedData.Lot}\n" +
                                      $"S/N: {parsedData.SerialNumber}\n" +
                                      $"EXP: {parsedData.ExpirationDate?.ToString("MM/dd/yyyy")}";

            await DisplayAlert("Compliant Item Logged", formattedMessage, "OK");

            // TODO: Insert into database using parsedData.Ndc

            // Resume scanning when ready
            // barcodeReader.IsDetecting = true;
        });
    }
}