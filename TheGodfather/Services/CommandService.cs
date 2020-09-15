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

        private readonly LocalizationService lcs;
        private readonly GuildConfigService gcs;
        private ImmutableDictionary<string, CommandInfo>? commands;
        private bool isDataLoaded;


        public CommandService(GuildConfigService gcs, LocalizationService lcs, bool loadData = true)
        {
            this.lcs = lcs;
            this.gcs = gcs;
            if (loadData)
                this.LoadData("Translations");
        }


        public void LoadData(string root)
        {
            var cmds = new Dictionary<string, CommandInfo>();

            string path = Path.Combine(root, "Commands");
            try {
                Log.Debug("Loading command info from {Folder}", path);
                foreach (FileInfo fi in new DirectoryInfo(path).EnumerateFiles("cmds_*.json", SearchOption.TopDirectoryOnly)) {
                    try {
                        string json = File.ReadAllText(fi.FullName);
                        foreach ((string cmd, CommandInfo info) in JsonConvert.DeserializeObject<Dictionary<string, CommandInfo>>(json)) {
                            foreach (string arg in info.UsageExamples.SelectMany(e => e)) {
                                try {
                                    string _ = this.lcs.GetString(null, arg);
                                    Log.Verbose("Checked {Argument}", arg);
                                } catch (LocalizationException) {
                                    Log.Warning("Failed to find translation for command argument {Argument} in examples of command {Command}", arg, cmd);
                                }
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

            if (!cmds.Any())
                throw new IOException("No valid JSON files found");

            this.commands = cmds.ToImmutableDictionary();
            this.isDataLoaded = true;
        }

        public string GetCommandDescription(ulong gid, string command)
            => this.lcs.GetCommandDescription(gid, command);

        public IReadOnlyList<string> GetCommandUsageExamples(ulong gid, string command)
        {
            CommandInfo cmdInfo = this.GetInfoForCommand(command);
            string locale = this.lcs.GetGuildLocale(gid);
            var examples = new List<string>();

            if (cmdInfo.UsageExamples.Any()) {
                foreach (List<string> args in cmdInfo.UsageExamples) {
                    string cmd = $"{this.gcs.GetGuildPrefix(gid)}{command}";
                    if (args.Any())
                        examples.Add($"{cmd} {string.Join(" ", args.Select(arg => this.lcs.GetString(gid, arg)))}");
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
                throw new KeyNotFoundException($"Failed to find info for {command}");

            return cmdInfo;
        }


        private sealed class CommandInfo
        {
            [JsonProperty("usage")]
            public List<List<string>> UsageExamples { get; set; } = new List<List<string>>();
        }
    }
}
