using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Assets;
using System;
using System.IO;
using System.Xml.Serialization;
using SDG.Unturned;

namespace Rocket.Core.Utils
{
    public static class TranslationSampleMerger
    {
        public static void TryMergeChineseFromSample(
            IAsset<TranslationList> translations,
            TranslationList englishDefaults,
            string languageCode,
            string translationFileName)
        {
            if (!string.Equals(LanguageCodeHelper.Normalize(languageCode), "zh", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string? samplePath = ConfigSampleHelper.GetSampleFilePath(translationFileName);
            if (samplePath == null || !File.Exists(samplePath))
            {
                return;
            }

            TranslationList? sample;
            try
            {
                XmlSerializer serializer = new XmlSerializer(
                    typeof(TranslationList),
                    new[] { typeof(TranslationList), typeof(TranslationListEntry) });
                using StreamReader reader = new StreamReader(samplePath, RocketFileEncoding.Utf8WithBom, detectEncodingFromByteOrderMarks: true);
                sample = (TranslationList?)serializer.Deserialize(reader);
            }
            catch (Exception)
            {
                return;
            }

            if (sample == null)
            {
                return;
            }

            bool changed = false;
            foreach (TranslationListEntry sampleEntry in sample)
            {
                if (string.IsNullOrEmpty(sampleEntry.Id))
                {
                    continue;
                }

                string? current = translations.Instance[sampleEntry.Id];
                string? englishDefault = englishDefaults[sampleEntry.Id];
                if (current == null)
                {
                    translations.Instance.Add(sampleEntry.Id, sampleEntry.Value);
                    changed = true;
                }
                else if (englishDefault != null
                    && string.Equals(current, englishDefault, StringComparison.Ordinal)
                    && !string.Equals(current, sampleEntry.Value, StringComparison.Ordinal))
                {
                    translations.Instance[sampleEntry.Id] = sampleEntry.Value;
                    changed = true;
                }
            }

            if (changed)
            {
                translations.Save();
            }
        }
    }
}
