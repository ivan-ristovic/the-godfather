using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Services
{
    public sealed class LocalizationService : ITheGodfatherService
    {
        public bool IsDisabled => false;
        public IReadOnlyList<string> AvailableLocales => this.strings.Keys.ToList().AsReadOnly();

        private ImmutableDictionary<string, ImmutableDictionary<string, string>> strings;
        private ImmutableDictionary<string, CommandInfo> commands;
        private readonly GuildConfigService gcs;
        private readonly DatabaseContextBuilder dbb;
        private readonly string defLocale;


        public LocalizationService(BotConfig cfg, GuildConfigService gcs, DatabaseContextBuilder dbb, string root)
        {
            this.gcs = gcs;
            this.dbb = dbb;
            this.defLocale = cfg.Locale;
            this.LoadData(root);
        }


        public void LoadData(string root)
        {
            TryLoadTranslations(root);
            TryLoadCommands(Path.Combine(root, "Commands"));

            if (!this.strings.ContainsKey(this.defLocale))
                throw new KeyNotFoundException($"The default locale {this.defLocale} is not loaded");


            void TryLoadCommands(string path)
            {
                var cmds = new Dictionary<string, CommandInfo>();

                Log.Debug("Loading command descriptions from {Folder}", path);
                try {
                    var desc = new Dictionary<string, Dictionary<string, string>>();
                    foreach (FileInfo fi in new DirectoryInfo(path).EnumerateFiles("desc_*.json", SearchOption.TopDirectoryOnly)) {
                        try {
                            string json = File.ReadAllText(fi.FullName);
                            string locale = fi.Name.Substring(5, fi.Name.IndexOf('.'));
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
                                    throw new IOException($"Duplicate command info: {cmd}");
                                foreach ((string locale, Dictionary<string, string> localeDesc) in desc) {
                                    if (localeDesc.TryGetValue(cmd, out string cmdDesc))
                                        info.Descriptions.Add(locale, cmdDesc);
                                    else
                                        Log.Warning("Cannot find description in locale {Locale} for command {CommandName}", locale, cmd);
                                }
                                cmds.Add(cmd, info);
                            }
                            Log.Debug("Loaded command list from: {FileName}", fi.Name);
                        } catch (JsonReaderException e) {
                            Log.Error(e, "Failed to load command list from file: {FileName}", fi.Name);
                        }
                    }
                } catch (IOException e) {
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

        public string GetString(ulong gid, string key)
        {
            string response = null;
            if (!string.IsNullOrWhiteSpace(key)) {
                string locale = this.gcs.GetCachedConfig(gid).Locale ?? this.defLocale;
                if (!this.strings[locale].TryGetValue(key, out response))
                    Log.Error(new KeyNotFoundException(), "Failed to find string for {Key} in locale {Locale}", key, locale);
            }

            return response ?? $"I do not have a translation ready for {key}. Please report this.";
        }

        public string GetGuildLocale(ulong gid)
            => this.gcs.GetCachedConfig(gid)?.Locale;

        public async Task<bool> SetGuildLocaleAsync(ulong gid, string locale)
        {
            if (!this.strings.ContainsKey(locale))
                return false;
            await this.gcs.ModifyConfigAsync(gid, cfg => cfg.Locale = locale);
            return true;
        }
    }


    public sealed class CommandInfo
    {
        [JsonProperty("examples")]
        public List<string> UsageExamples { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> Descriptions { get; set; } = new Dictionary<string, string>();
    }
}
