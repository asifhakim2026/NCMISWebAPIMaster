namespace NCMISAPI.Helpers;

public static class GlobalHelper
{
    public static string? ValidateCnicByType(string identificationType, string cnic)
    {
        if (string.IsNullOrWhiteSpace(identificationType))
            return "Identification type is required.";

        if (string.IsNullOrWhiteSpace(cnic))
            return "Identification number is required.";

        switch (identificationType.Trim().ToUpperInvariant())
        {
            case "CNIC":
            case "SNIC":
            case "NICOP":
                if (cnic.Length != 13 || !cnic.All(char.IsDigit))
                    return $"{identificationType} must be exactly 13 numeric digits.";
                break;

            case "BFORM":
                if (cnic.Length is < 11 or > 13 || !cnic.All(char.IsDigit))
                    return "B-Form must be between 11 to 13 numeric digits.";
                break;

            case "POC":
            case "FRC":
                if (cnic.Length is < 10 or > 16)
                    return $"{identificationType} must be between 10 to 16 characters.";
                break;

            default:
                return "Unsupported identification type selected.";
        }

        return null;
    }
}
