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
        public IReadOnlyList<string> AvailableLocales => this.strings.Keys.ToList().AsReadOnly();
        public IReadOnlyList<string> AvailableCommands => this.commands.Keys.ToList().AsReadOnly();

        private ImmutableDictionary<string, ImmutableDictionary<string, string>> strings;
        private ImmutableDictionary<string, CommandInfo> commands;
        private readonly GuildConfigService gcs;
        private readonly string defLocale;
        private bool isDataLoaded;


        public LocalizationService(GuildConfigService gcs, BotConfigService cfg, bool loadData = true)
        {
            this.gcs = gcs;
            this.defLocale = cfg.CurrentConfiguration.Locale;
            if (loadData)
                this.LoadData("Translations");
        }


        public void LoadData(string root)
        {
            TryLoadTranslations(root);
            TryLoadCommands(Path.Combine(root, "Commands"));

            if (!this.strings.ContainsKey(this.defLocale))
                throw new LocalizationException($"The default locale {this.defLocale} is not loaded");

            this.isDataLoaded = true;


            void TryLoadCommands(string path)
            {
                var cmds = new Dictionary<string, CommandInfo>();

                Log.Debug("Loading command descriptions from {Folder}", path);
                try {
                    var desc = new Dictionary<string, Dictionary<string, string>>();
                    foreach (FileInfo fi in new DirectoryInfo(path).EnumerateFiles("desc_*.json", SearchOption.TopDirectoryOnly)) {
                        try {
                            string json = File.ReadAllText(fi.FullName);
                            string locale = fi.Name.Substring(5, fi.Name.IndexOf('.') - 5);
                            Dictionary<string, string> localeDesc = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                            desc.Add(locale, localeDesc);
                            Log.Debug("Loaded descriptions for locale: {Locale}", locale);
                        } catch (JsonReaderException e) {
                            Log.Error(e, "Failed to load descriptions from file: {FileName}", fi.Name);
                        }
                    }

                    Log.Debug("Loading command info from {Folder}", path);
                    foreach (FileInfo fi in new DirectoryInfo(path).EnumerateFiles("cmds_*.json", SearchOption.TopDirectoryOnly)) {
                        try {
                            string json = File.ReadAllText(fi.FullName);
                            Dictionary<string, CommandInfo> cmdsPart = JsonConvert.DeserializeObject<Dictionary<string, CommandInfo>>(json);

                            foreach ((string cmd, CommandInfo info) in cmdsPart) {
                                if (cmds.ContainsKey(cmd))
                                    throw new LocalizationException($"Duplicate command info: {cmd}");
                                foreach ((string locale, Dictionary<string, string> localeDesc) in desc) {
                                    if (localeDesc.TryGetValue(cmd, out string? cmdDesc))
                                        info.Descriptions.Add(locale, cmdDesc);
                                    else
                                        Log.Error("Cannot find description in locale {Locale} for command {CommandName}", locale, cmd);
                                }
                                cmds.Add(cmd, info);
                            }
                            Log.Debug("Loaded command list from: {FileName}", fi.Name);
                        } catch (JsonReaderException e) {
                            Log.Error(e, "Failed to load command list from file: {FileName}", fi.Name);
                        }
                    }
                } catch (Exception e) {
                    Log.Fatal(e, "Failed to load command translations");
                    throw e;
                }

                this.commands = cmds.ToImmutableDictionary();
                Log.Information("Loaded command translations");
            }

            void TryLoadTranslations(string path)
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
            }
        }

        public string GetString(ulong? gid, string key, params object[]? args)
        {
            this.AssertIsDataLoaded();

            string? response = null;
            try {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                string locale = this.GetGuildLocale(gid);
                if (!this.strings[locale].TryGetValue(key, out response)) {
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
            return gid is null ? this.defLocale : this.gcs.GetCachedConfig(gid.Value)?.Locale ?? this.defLocale;
        }

        public CultureInfo GetGuildCulture(ulong? gid)
        {
            var defCulture = new CultureInfo(this.defLocale);
            return gid is null ? defCulture : this.gcs.GetCachedConfig(gid.Value)?.Culture ?? defCulture;
        }

        public string GetCommandDescription(ulong gid, string command)
        {
            CommandInfo cmdInfo = this.GetInfoForCommand(command);
            string locale = this.GetGuildLocale(gid);

            if (!cmdInfo.Descriptions.TryGetValue(locale, out string? desc) && !cmdInfo.Descriptions.TryGetValue(this.defLocale, out desc))
                throw new LocalizationException("No translations found in either guild or default locale for given command.");

            return desc;
        }

        public string GetLocalizedTime(ulong gid, DateTimeOffset? dt = null, string format = "g")
        {
            CachedGuildConfig gcfg = this.gcs.GetCachedConfig(gid) ?? new CachedGuildConfig();
            DateTimeOffset time = dt ?? DateTimeOffset.Now;
            time = TimeZoneInfo.ConvertTime(time, TimeZoneInfo.FindSystemTimeZoneById(gcfg.TimezoneId));
            return time.ToString(format, gcfg.Culture);
        }

        public IReadOnlyList<string> GetCommandUsageExamples(ulong gid, string command)
        {
            CommandInfo cmdInfo = this.GetInfoForCommand(command);
            string locale = this.GetGuildLocale(gid);
            var examples = new List<string>();

            if (cmdInfo.UsageExamples.Any()) {
                foreach (List<string> args in cmdInfo.UsageExamples) {
                    string cmd = $"{this.gcs.GetGuildPrefix(gid)}{command}";
                    if (args.Any())
                        examples.Add($"{cmd} {string.Join(" ", args.Select(arg => this.GetString(gid, arg)))}");
                    else
                        examples.Add(cmd);
                }
            }

            return examples.AsReadOnly();
        }

        public async Task<bool> SetGuildLocaleAsync(ulong gid, string locale)
        {
            this.AssertIsDataLoaded();

            if (!this.strings.ContainsKey(locale))
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
            if (!this.isDataLoaded)
                throw new InvalidOperationException("The translation data has not been loaded.");
        }

        private CommandInfo GetInfoForCommand(string command)
        {
            this.AssertIsDataLoaded();

            if (!this.commands.TryGetValue(command, out CommandInfo cmdInfo))
                throw new LocalizationException("No translations for this command have been found.");

            return cmdInfo;
        }


        private sealed class CommandInfo
        {
            [JsonProperty("usage")]
            public List<List<string>> UsageExamples { get; set; } = new List<List<string>>();

            [JsonIgnore]
            public Dictionary<string, string> Descriptions { get; set; } = new Dictionary<string, string>();
        }
    }
}
