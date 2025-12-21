using System;
using System.Collections.Generic;

namespace Runtime.Localization
{
    public sealed class AddressablesLocalizationCatalog : ILocalizationCatalog
    {
        private static readonly LocaleId[] _supportedLocales =
        {
            LocaleId.EnglishUs,
            LocaleId.Russian,
            LocaleId.Japanese,
        };

        private static readonly TextTableId[] _startupTables =
        {
            TextTableId.UI,
            TextTableId.Errors,
        };

        public IReadOnlyList<LocaleId> GetSupportedLocales() => _supportedLocales;
        public IReadOnlyList<TextTableId> GetStartupTables() => _startupTables;

        public string GetAssetKey(LocaleId locale, TextTableId table)
        {
            // Convention-based, deterministic mapping.
            // Uses full locale (including region) to avoid collisions like en-US vs en-GB.
            // Example: "loc_en_us_ui".
            var localeToken = NormalizeLocaleToken(locale);
            var tableName = table.Name.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(localeToken))
                throw new ArgumentException($"Invalid locale: '{locale.Code}'", nameof(locale));

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Invalid table.", nameof(table));

            return $"loc_{localeToken}_{tableName}";
        }

        private static string NormalizeLocaleToken(LocaleId locale)
        {
            var code = locale.Code;
            
            if (string.IsNullOrWhiteSpace(code))
                return string.Empty;

            return code.Trim().ToLowerInvariant().Replace('-', '_');
        }
    }
}