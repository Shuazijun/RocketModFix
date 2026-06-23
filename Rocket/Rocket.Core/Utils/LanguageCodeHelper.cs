using System;

namespace Rocket.Core.Utils
{
    public static class LanguageCodeHelper
    {
        /// <summary>
        /// Normalizes Rocket translation language codes (Rocket.{code}.translation.xml).
        /// Unturned module UI uses Schinese.dat separately from this setting.
        /// </summary>
        public static string Normalize(string? languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return "en";
            }

            string code = languageCode!.Trim().ToLowerInvariant();
            switch (code)
            {
                case "schinese":
                case "zh-cn":
                case "zh_cn":
                case "zh-hans":
                case "zh_hans":
                case "chs":
                    return "zh";
                default:
                    return code;
            }
        }
    }
}
