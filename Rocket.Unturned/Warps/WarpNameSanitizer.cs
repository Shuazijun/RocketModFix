using System.Text.RegularExpressions;

namespace Rocket.Unturned.Warps
{
    internal static class WarpNameSanitizer
    {
        public static string Sanitize(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            return Regex.Replace(value, @"([\u0000-\u001F])+", " ").Trim();
        }
    }
}
