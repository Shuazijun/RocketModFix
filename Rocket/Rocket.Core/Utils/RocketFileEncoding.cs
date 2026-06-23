using System.Text;

namespace Rocket.Core.Utils
{
    internal static class RocketFileEncoding
    {
        /// <summary>
        /// UTF-8 with BOM — improves compatibility with Windows Notepad and Chinese locales.
        /// </summary>
        public static readonly Encoding Utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
    }
}
