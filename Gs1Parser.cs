using System;

namespace PharmacyInventoryOptimizer;

public class Gs1Data
{
    public string Gtin { get; set; }
    public string Lot { get; set; }
    public string SerialNumber { get; set; }
    public string RawExpiration { get; set; }

    // --- NEW: Proper NDC Extraction ---
    public string Ndc
    {
        get
        {
            if (string.IsNullOrEmpty(Gtin) || Gtin.Length != 14) return null;

            // U.S. NDCs in GTIN-14 are formatted as: [Packaging Indicator] + [03] + [10-Digit NDC] + [Check Digit]
            // Example GTIN: 1 03 1234567890 6
            if (Gtin.Substring(1, 2) == "03")
            {
                return Gtin.Substring(3, 10);
            }

            // If the prefix isn't 03, return the whole GTIN (might be a global or non-standard code)
            return Gtin;
        }
    }

    // --- NEW: DSCSA Compliance Check ---
    // The FDA's DSCSA requires all 4 elements to be present for a drug to be valid
    public bool IsDscsaCompliant =>
        !string.IsNullOrEmpty(Gtin) &&
        !string.IsNullOrEmpty(Lot) &&
        !string.IsNullOrEmpty(SerialNumber) &&
        ExpirationDate.HasValue;

    public DateTime? ExpirationDate
    {
        get
        {
            if (string.IsNullOrEmpty(RawExpiration) || RawExpiration.Length != 6) return null;
            try
            {
                int year = 2000 + int.Parse(RawExpiration.Substring(0, 2));
                int month = int.Parse(RawExpiration.Substring(2, 2));
                int day = int.Parse(RawExpiration.Substring(4, 2));

                if (day == 0)
                {
                    day = DateTime.DaysInMonth(year, month);
                }

                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }
    }
}

public static class Gs1Parser
{
    public static Gs1Data Parse(string barcodeData)
    {
        var result = new Gs1Data();
        if (string.IsNullOrWhiteSpace(barcodeData)) return result;

        // --- NEW: Strip GS1 Symbology Identifiers ---
        // Hardware/Software scanners often prepend "]d2" to indicate a GS1 DataMatrix
        if (barcodeData.StartsWith("]d2", StringComparison.OrdinalIgnoreCase) ||
            barcodeData.StartsWith("]d1", StringComparison.OrdinalIgnoreCase))
        {
            barcodeData = barcodeData.Substring(3);
        }

        int index = 0;
        char groupSeparator = (char)29; // ASCII 29 / FNC1

        while (index < barcodeData.Length)
        {
            if (barcodeData[index] == groupSeparator)
            {
                index++;
                continue;
            }

            if (index + 2 > barcodeData.Length) break;

            string ai = barcodeData.Substring(index, 2);
            index += 2;

            switch (ai)
            {
                case "01": // GTIN (14 digits)
                    if (index + 14 <= barcodeData.Length)
                    {
                        result.Gtin = barcodeData.Substring(index, 14);
                        index += 14;
                    }
                    break;

                case "17": // Expiration Date (6 digits)
                    if (index + 6 <= barcodeData.Length)
                    {
                        result.RawExpiration = barcodeData.Substring(index, 6);
                        index += 6;
                    }
                    break;

                case "10": // LOT Number (Variable up to 20 chars)
                case "21": // Serial Number (Variable up to 20 chars)
                    int endPos = barcodeData.IndexOf(groupSeparator, index);
                    if (endPos == -1) endPos = barcodeData.Length;

                    string value = barcodeData.Substring(index, endPos - index);

                    if (ai == "10") result.Lot = value;
                    else if (ai == "21") result.SerialNumber = value;

                    index = endPos;
                    break;

                default:
                    // Unhandled AI -> Skip to the next FNC1 separator
                    int skipPos = barcodeData.IndexOf(groupSeparator, index);
                    index = skipPos == -1 ? barcodeData.Length : skipPos;
                    break;
            }
        }

        return result;
    }
}