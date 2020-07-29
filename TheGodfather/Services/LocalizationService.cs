using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Services
{
    public sealed class LocalizationService : ITheGodfatherService
    {
        private static readonly HashSet<string> _tzIds = new HashSet<string>(TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id));

        public bool IsDisabled => false;
        public string DefaultLocale { get; }
        public IReadOnlyList<string> AvailableLocales => this.strings?.Keys.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();

        private ImmutableDictionary<string, ImmutableDictionary<string, string>>? strings;
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
            var strs = new Dictionary<string, ImmutableDictionary<string, string>>();

            Log.Debug("Loading strings from {Folder}", path);
            try {
                foreach (FileInfo fi in new DirectoryInfo(path).EnumerateFiles("*.json", SearchOption.TopDirectoryOnly)) {
                    try {
                        string json = File.ReadAllText(fi.FullName);
                        string locale = fi.Name.Substring(0, fi.Name.IndexOf('.'));
                        Dictionary<string, string> translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        strs.Add(locale, translation.ToImmutableDictionary());
                        Log.Debug("Loaded locale: {Locale}", locale);
                    } catch (Exception e) {
                        Log.Error(e, "Failed to load locale: {LocaleJson}", fi.Name);
                    }
                }
            } catch (IOException e) {
                Log.Fatal(e, "Failed to load strings");
                throw e;
            }

            this.strings = strs.ToImmutableDictionary();
            if (!strs.Any())
                throw new IOException("No valid strings json files loaded");

            Log.Information("Loaded strings");

            if (!this.strings.ContainsKey(this.DefaultLocale))
                throw new LocalizationException($"The default locale {this.DefaultLocale} is not loaded");

            this.isDataLoaded = true;
        }

        public string GetString(ulong? gid, string key, params object[]? args)
        {
            this.AssertIsDataLoaded();

            string? response = null;
            try {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                string locale = this.GetGuildLocale(gid);
                if (!this.strings![locale].TryGetValue(key, out response)) {
                    Log.Error("Failed to find string for {Key} in locale {Locale}", key, locale);
                    throw new LocalizationException($"I do not have a translation ready for `{key}`. Please report this.");
                }

                return string.Format(response ?? "Translation error. Please report this", args ?? new object[] { });
            } catch (KeyNotFoundException e) {
                Log.Error(e, "Locale not found for guild {Guild}", gid);
            }

            return string.Format(response ?? "Error. Please report this", args ?? new object[] { });
        }
        
        public string GetGuildLocale(ulong? gid)
        {
            return gid is null ? this.DefaultLocale : this.gcs.GetCachedConfig(gid.Value)?.Locale ?? this.DefaultLocale;
        }

        public CultureInfo GetGuildCulture(ulong? gid)
        {
            var defCulture = new CultureInfo(this.DefaultLocale);
            return gid is null ? defCulture : this.gcs.GetCachedConfig(gid.Value)?.Culture ?? defCulture;
        }

        public string GetLocalizedTime(ulong gid, DateTimeOffset? dt = null, string format = "g")
        {
            CachedGuildConfig gcfg = this.gcs.GetCachedConfig(gid) ?? new CachedGuildConfig();
            DateTimeOffset time = dt ?? DateTimeOffset.Now;
            time = TimeZoneInfo.ConvertTime(time, TimeZoneInfo.FindSystemTimeZoneById(gcfg.TimezoneId));
            return time.ToString(format, gcfg.Culture);
        }

        public async Task<bool> SetGuildLocaleAsync(ulong gid, string locale)
        {
            this.AssertIsDataLoaded();

            if (!this.strings!.ContainsKey(locale))
                return false;
            await this.gcs.ModifyConfigAsync(gid, cfg => cfg.Locale = locale);
            return true;
        }

        public async Task<bool> SetGuildTimezoneIdAsync(ulong gid, string tzid)
        {
            if (!_tzIds.Contains(tzid))
                return false;
            await this.gcs.ModifyConfigAsync(gid, cfg => cfg.TimezoneId = tzid);
            return true;
        }


        private void AssertIsDataLoaded()
        {
            if (!this.isDataLoaded || this.strings is null)
                throw new InvalidOperationException("The translation data has not been loaded.");
        }
    }
}
