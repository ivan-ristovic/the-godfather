using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Humanizer;
using Newtonsoft.Json;
using Serilog;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Services
{
    public sealed class CommandService : ITheGodfatherService
    {
        public bool IsDisabled => false;
        public IReadOnlyList<string> AvailableCommands => this.commands?.Keys.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();

        private readonly LocalizationService lcs;
        private readonly GuildConfigService gcs;
        private ImmutableDictionary<string, CommandInfo>? commands;
        private bool isDataLoaded;


        public CommandService(GuildConfigService gcs, LocalizationService lcs, bool loadData = true)
        {
            this.lcs = lcs;
            this.gcs = gcs;
            if (loadData)
                this.LoadData(Path.Combine("Translations", "Commands"));
        }


        public void LoadData(string path)
        {
            var cmds = new Dictionary<string, CommandInfo>();

            Log.Debug("Loading command descriptions from {Folder}", path);
            try {
                var desc = new Dictionary<string, Dictionary<string, string>>();
                foreach (FileInfo fi in new DirectoryInfo(path).EnumerateFiles("desc_*.json", SearchOption.TopDirectoryOnly)) {
                    try {
                        string json = File.ReadAllText(fi.FullName);
                        string locale = fi.Name[5..fi.Name.IndexOf('.')];
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
            this.isDataLoaded = true;
        }

        public string GetCommandDescription(ulong gid, string command)
        {
            CommandInfo cmdInfo = this.GetInfoForCommand(command);
            string locale = this.lcs.GetGuildLocale(gid);

            if (!cmdInfo.Descriptions.TryGetValue(locale, out string? desc) && !cmdInfo.Descriptions.TryGetValue(this.lcs.DefaultLocale, out desc))
                throw new LocalizationException("No translations found in either guild or default locale for given command.");

            return desc;
        }

        public IReadOnlyList<string> GetCommandUsageExamples(ulong gid, string command)
        {
            CommandInfo cmdInfo = this.GetInfoForCommand(command);
            string locale = this.lcs.GetGuildLocale(gid);
            var examples = new List<string>();

            if (cmdInfo.UsageExamples.Any()) {
                foreach (List<string> args in cmdInfo.UsageExamples) {
                    string cmd = $"{this.gcs.GetGuildPrefix(gid)}{command}";
                    if (args.Any())
                        examples.Add($"{cmd} {args.Select(arg => this.lcs.GetString(gid, arg)).Humanize(" ")}");
                    else
                        examples.Add(cmd);
                }
            }

            return examples.AsReadOnly();
        }


        private void AssertIsDataLoaded()
        {
            if (!this.isDataLoaded || this.commands is null)
                throw new InvalidOperationException("The translation data has not been loaded.");
        }

        private CommandInfo GetInfoForCommand(string command)
        {
            this.AssertIsDataLoaded();

            if (!this.commands!.TryGetValue(command, out CommandInfo cmdInfo))
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
