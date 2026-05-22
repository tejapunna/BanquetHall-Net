namespace BanquetHall.Helpers;

public static class AadhaarHelper
{
    /// <summary>
    /// Masks Aadhaar number based on user role.
    /// Receptionist sees only last 4 digits (XXXXXXXX + last 4).
    /// Admin and Manager see the full value.
    /// </summary>
    public static string MaskAadhaar(string? aadhaar, string role)
    {
        if (string.IsNullOrEmpty(aadhaar))
            return "";

        if (string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase))
        {
            if (aadhaar.Length >= 4)
                return "XXXXXXXX" + aadhaar[^4..];
            return "XXXXXXXX" + aadhaar;
        }

        return aadhaar;
    }
}
