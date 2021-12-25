using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Newtonsoft.Json;
using Serilog;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;
using TheGodfather.Generators;
using TimeZoneConverter;

namespace TheGodfather.Services
{
    public sealed class LocalizationService : ITheGodfatherService
    {
        private static readonly HashSet<string> _tzIds = new HashSet<string>(TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id));

        public bool IsDisabled => false;
        public string DefaultLocale { get; }
        public IReadOnlyList<string> AvailableLocales => this.strings?.Keys.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();

        private ConcurrentDictionary<string, ImmutableDictionary<string, string>>? strings;
        private ConcurrentDictionary<string, ImmutableDictionary<string, string>>? cmddesc;
        private readonly GuildConfigService gcs;
        private bool isDataLoaded;


        public LocalizationService(GuildConfigService gcs, BotConfigService cfg, bool loadData = true)
        {
            this.gcs = gcs;
            this.DefaultLocale = cfg.CurrentConfiguration.Locale;
            if (loadData)
                this.LoadData("Translations");
        }


        public void LoadData(string path)
        {
            Log.Debug("Loading translation strings from {Path}", path);
            this.strings = new ConcurrentDictionary<string, ImmutableDictionary<string, string>>(ReadStrings(path, "*.json"));
            Log.Information("Loaded translation strings");

            path = Path.Combine(path, "Commands");
            Log.Debug("Loading command descriptions from {Path}", path);
            this.cmddesc = new ConcurrentDictionary<string, ImmutableDictionary<string, string>>(ReadStrings(path, "desc_*.json"));
            Log.Information("Loaded command descriptions");

            this.isDataLoaded = true;


            Dictionary<string, ImmutableDictionary<string, string>> ReadStrings(string stringsPath, string searchPattern)
            {
                var strs = new Dictionary<string, ImmutableDictionary<string, string>>();

                bool defLocaleLoaded = false;

                try {
                    foreach (FileInfo fi in new DirectoryInfo(stringsPath).EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly)) {
                        try {
                            string json = File.ReadAllText(fi.FullName);
                            string locale = fi.Name[searchPattern.IndexOf('*')..fi.Name.IndexOf('.')];
                            if (locale.Equals(this.DefaultLocale))
                                defLocaleLoaded = true;
                            Dictionary<string, string>? translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                            if (translation is null || !strs.TryAdd(locale, translation.ToImmutableDictionary()))
                                Log.Error("Duplicate locale found in lookup table: {Locale}", locale);
                        } catch (Exception e) {
                            Log.Error(e, "Failed to load strings for locale: {LocaleJson}", fi.Name);
                        }
                    }
                } catch (IOException e) {
                    Log.Fatal(e, "Failed to load strings from path {Path}", stringsPath);
                    throw;
                }

                if (!strs.Any())
                    throw new IOException("No valid JSON files found");

                if (!defLocaleLoaded)
                    throw new IOException($"Default locale {this.DefaultLocale} is not loaded");

                return strs;
            }
        }

        public string GetString(ulong? gid, string key, params object?[]? args)
        {
            this.AssertIsDataLoaded();

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            string locale = this.GetGuildLocale(gid);
            if (this.strings!.TryGetValue(locale, out ImmutableDictionary<string, string>? localeStrings)) {
                if (!localeStrings.TryGetValue(key, out string? response)) {
                    Log.Error("Failed to find string for {Key} in locale {Locale}", key, locale);
                    throw new LocalizationException($"I do not have a translation ready for:{Formatter.InlineCode(key)} Please report this.");
                }
                if (!localeStrings.TryGetValue("str-404", out string? str404)) {
                    Log.Error("Failed to find string for {Key} in locale {Locale}", "str-404", locale);
                    throw new LocalizationException($"I do not have a translation ready for:{Formatter.InlineCode("str-404")} Please report this.");
                }
                IEnumerable<object> margs = args?.Select(a => a is null ? str404 : a) ?? Enumerable.Empty<object>();
                return string.Format(response, margs.ToArray()).Trim();
            } else {
                Log.Error("Guild {GuildId} has unknown locale {Locale}", gid, locale);
                throw new LocalizationException($"Locale not found for guild {gid}");
            }
        }
        
        public string GetString(ulong? gid, TranslationKey key)
        {
            this.AssertIsDataLoaded();

            string locale = this.GetGuildLocale(gid);
            if (this.strings!.TryGetValue(locale, out ImmutableDictionary<string, string>? localeStrings)) {
                if (!localeStrings.TryGetValue(key.Key, out string? response)) {
                    Log.Error("Failed to find string for {Key} in locale {Locale}", key, locale);
                    throw new LocalizationException($"I do not have a translation ready for:{Formatter.InlineCode(key.Key)} Please report this.");
                }

                string unknownKey = TranslationKey.NotFound.Key;
                if (!localeStrings.TryGetValue(unknownKey, out string? str404)) {
                    Log.Error("Failed to find string for {Key} in locale {Locale}", unknownKey, locale);
                    throw new LocalizationException($"I do not have a translation ready for:{Formatter.InlineCode(unknownKey)} Please report this.");
                }
                IEnumerable<object> margs = key.Params.Any() ? key.Params.Select(a => a ?? str404) : Enumerable.Empty<object>();
                return string.Format(response, margs.ToArray()).Trim();
            }

            Log.Error("Guild {GuildId} has unknown locale {Locale}", gid, locale);
            throw new LocalizationException($"Locale not found for guild {gid}");
        }

        public string GetCommandDescription(ulong? gid, string command)
        {
            this.AssertIsDataLoaded();

            string locale = this.GetGuildLocale(gid);

            ImmutableDictionary<string, string>? desc = null;
            if (!this.cmddesc?.TryGetValue(locale, out desc) ?? true)
                throw new LocalizationException($"Failed to find locale {locale}");

            if (desc == null || !desc.TryGetValue(command, out string? localizedDesc))
                throw new LocalizationException($"Failed to find description for command `{command}` in locale `{locale}`");

            return localizedDesc;
        }

        public string GetGuildLocale(ulong? gid)
        {
            return gid is null ? this.DefaultLocale : this.gcs.GetCachedConfig(gid.Value).Locale;
        }

        public CultureInfo GetGuildCulture(ulong? gid)
        {
            var defCulture = new CultureInfo(this.DefaultLocale);
            return gid is null ? defCulture : this.gcs.GetCachedConfig(gid.Value).Culture;
        }
        
        public DateTimeOffset GetLocalizedTime(ulong? gid, DateTimeOffset? dt = null)
        {
            CachedGuildConfig gcfg = this.gcs.GetCachedConfig(gid);
            DateTimeOffset time = dt ?? DateTimeOffset.Now;
            return TimeZoneInfo.ConvertTime(time, TZConvert.GetTimeZoneInfo(gcfg.TimezoneId));
        }

        public string GetLocalizedTimeString(ulong? gid, DateTimeOffset? dt = null, string format = "g", bool unknown = false)
        {
            if (unknown && dt is null)
                return this.GetString(gid, "str-404");
            CachedGuildConfig gcfg = this.gcs.GetCachedConfig(gid);
            return this.GetLocalizedTime(gid, dt).ToString(format, gcfg.Culture);
        }

        public async Task<bool> SetGuildLocaleAsync(ulong gid, string locale)
        {
            this.AssertIsDataLoaded();

            if (!this.strings!.ContainsKey(locale))
                return false;
            await this.gcs.ModifyConfigAsync(gid, cfg => cfg.Locale = locale);
            return true;
        }

        public TimeZoneInfo GetGuildTimeZone(ulong? gid)
        {
            if (gid is null)
                return TimeZoneInfo.Utc;

            CachedGuildConfig gcfg = this.gcs.GetCachedConfig(gid.Value);
            try {
                return TZConvert.GetTimeZoneInfo(gcfg.TimezoneId);
            } catch (TimeZoneNotFoundException e) {
                Log.Error(e, "Timezone ID {TimezoneId} for guild {GuildId} is invalid!", gcfg.TimezoneId, gid);
            }

            return TimeZoneInfo.Utc;
        }

        public async Task<bool> SetGuildTimezoneIdAsync(ulong gid, string tzid)
        {
            if (!_tzIds.Contains(tzid))
                return false;
            await this.gcs.ModifyConfigAsync(gid, cfg => cfg.TimezoneId = tzid);
            return true;
        }

        public bool RemoveCommandDescription(string command)
        {
            this.AssertIsDataLoaded();
            return this.cmddesc?.TryRemove(command, out _) ?? true;
        }


        private void AssertIsDataLoaded()
        {
            if (!this.isDataLoaded || this.strings is null)
                throw new InvalidOperationException("The translation data has not been loaded.");
        }
    }
}
