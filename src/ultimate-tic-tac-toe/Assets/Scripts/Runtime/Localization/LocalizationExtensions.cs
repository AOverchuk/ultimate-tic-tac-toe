using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace Runtime.Localization
{
    public static class LocalizationExtensions
    {
        public static string Resolve(
            this ILocalizationService service,
            string table,
            string key,
            IReadOnlyDictionary<string, object> args = null) =>
            service.Resolve(new TextTableId(table), new TextKey(key), args);

        public static Observable<string> Observe(
            this ILocalizationService service,
            string table,
            string key,
            IReadOnlyDictionary<string, object> args = null) =>
            service.Observe(new TextTableId(table), new TextKey(key), args);

        public static UniTask PreloadCurrentLocaleAsync(
            this ILocalizationService service,
            TextTableId table,
            CancellationToken cancellationToken) =>
            service.PreloadAsync(service.CurrentLocale.CurrentValue, new[] { table }, cancellationToken);

        public static UniTask PreloadCurrentLocaleAsync(
            this ILocalizationService service,
            IReadOnlyList<TextTableId> tables,
            CancellationToken cancellationToken) =>
            service.PreloadAsync(service.CurrentLocale.CurrentValue, tables, cancellationToken);
    }
}
