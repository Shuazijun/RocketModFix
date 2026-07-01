using System;
using System.IO;
using SDG.Unturned;

namespace Rocket.Core.Utils
{
    internal static class ConfigSampleHelper
    {
        internal const string SampleRelativePath = "Modules/Rocket.Unturned/ConfigSamples";

        public static string? GetSampleFilePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            return Path.Combine(ReadWrite.PATH, SampleRelativePath, Path.GetFileName(fileName));
        }

        public static bool TryCopySample(string destinationFile)
        {
            try
            {
                if (string.IsNullOrEmpty(destinationFile) || File.Exists(destinationFile))
                {
                    return false;
                }

                string fileName = Path.GetFileName(destinationFile);
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                string samplePath = GetSampleFilePath(fileName) ?? string.Empty;
                if (!File.Exists(samplePath))
                {
                    return false;
                }

                string? directory = Path.GetDirectoryName(destinationFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Copy(samplePath, destinationFile);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
