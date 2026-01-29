using System.Globalization;
using System.Text.RegularExpressions;

namespace RuralAddress.Core.Helpers;

public static class CoordinateHelper
{
    // Regex for parsing: matches "S 22° 10' 52.3"" or "W 046° 31' 54.7""
    // Groups: 1=Direction, 2=Degrees, 3=Minutes, 4=Seconds
    private static readonly Regex DmsRegex = new Regex(
        @"^([NSWE])\s*(\d{1,3})°\s*(\d{1,2})'\s*(\d+(\.\d+)?)(" + "\"" + ")?$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static decimal? ConvertToDecimal(string? dmsInput)
    {
        if (string.IsNullOrWhiteSpace(dmsInput)) return null;

        var match = DmsRegex.Match(dmsInput.Trim());
        if (!match.Success)
        {
            throw new FormatException("Formato de coordenada inválido. Use o padrão: S 22° 10' 52.3\"");
        }

        string direction = match.Groups[1].Value.ToUpper();
        int degrees = int.Parse(match.Groups[2].Value);
        int minutes = int.Parse(match.Groups[3].Value);
        decimal seconds = decimal.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);

        decimal decimalValue = degrees + (minutes / 60m) + (seconds / 3600m);

        if (direction == "S" || direction == "W")
        {
            decimalValue *= -1;
        }

        return Math.Round(decimalValue, 8); // Standard precision
    }

    public static string? ConvertToDms(decimal? decimalDegree, bool isLatitude)
    {
        if (!decimalDegree.HasValue) return null;

        decimal val = decimalDegree.Value;
        string direction;

        if (isLatitude)
        {
            direction = val < 0 ? "S" : "N";
        }
        else
        {
            direction = val < 0 ? "W" : "E";
        }

        val = Math.Abs(val);

        int degrees = (int)val;
        decimal remaining = (val - degrees) * 60;
        int minutes = (int)remaining;
        decimal seconds = (remaining - minutes) * 60;

        // Round seconds to 1 decimal place for cleaner display, similar to user example
        seconds = Math.Round(seconds, 1);

        // Format: S 22° 10' 52.3"
        return $"{direction} {degrees}° {minutes}' {seconds.ToString("0.0", CultureInfo.InvariantCulture)}\"";
    }
}
