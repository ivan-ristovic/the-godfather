using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Serilog;
using TheGodfather.Attributes;
using TheGodfather.Common.Collections;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Services
{
    public sealed class CommandService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly LocalizationService lcs;
        private readonly GuildConfigService gcs;
        private ConcurrentDictionary<string, CommandInfo> commands;
        private ImmutableDictionary<ModuleType, ConcurrentHashSet<string>> modules;
        private bool isDataLoaded;


        public CommandService(GuildConfigService gcs, LocalizationService lcs, bool loadData = true)
        {
            this.lcs = lcs;
            this.gcs = gcs;
            this.commands = new ConcurrentDictionary<string, CommandInfo>();
            this.modules = new Dictionary<ModuleType, ConcurrentHashSet<string>>().ToImmutableDictionary();
            if (loadData)
                this.LoadData("Translations");
        }


        public void LoadData(string root)
        {
            var commands = new Dictionary<string, CommandInfo>();
            var modules = new Dictionary<ModuleType, ConcurrentHashSet<string>>();

            string path = Path.Combine(root, "Commands");
            try {
                Log.Debug("Loading command info from {Folder}", path);
                foreach (FileInfo fi in new DirectoryInfo(path).EnumerateFiles("cmds_*.json", SearchOption.TopDirectoryOnly)) {
                    ModuleType module = ParseModuleType(fi);
                    ReadCommands(fi, module);
                }
            } catch (Exception e) {
                Log.Fatal(e, "Failed to load command translations");
                throw;
            }

            if (!commands.Any())
                throw new IOException("No valid JSON files found");

            this.isDataLoaded = true;
            this.commands = new ConcurrentDictionary<string, CommandInfo>(commands);
            this.modules = modules.ToImmutableDictionary();


            ModuleType ParseModuleType(FileInfo fi)
            {
                string moduleRaw = fi.Name.Substring(0, fi.Name.IndexOf('.')).Substring(fi.Name.IndexOf('_') + 1);
                if (!Enum.TryParse(moduleRaw, out ModuleType module)) {
                    module = ModuleType.Uncategorized;
                    Log.Error("Failed to parse module name from file {FileName}", fi.Name);
                }
                if (!modules.ContainsKey(module))
                    modules.Add(module, new ConcurrentHashSet<string>());
                return module;
            }

            void ReadCommands(FileInfo fi, ModuleType module)
            {
                try {
                    string json = File.ReadAllText(fi.FullName);
                    foreach ((string cmd, CommandInfo info) in JsonConvert.DeserializeObject<Dictionary<string, CommandInfo>>(json)) {

                        info.Module = module;
                        foreach (string arg in info.UsageExamples.SelectMany(e => e)) {
                            try {
                                string _ = this.lcs.GetString(null, arg);
                                Log.Verbose("Checked {Argument}", arg);
                            } catch (LocalizationException) {
                                Log.Warning("Failed to find translation for command argument {Argument} in examples of command {Command}", arg, cmd);
                            }
                        }
                        if (!commands.TryAdd(cmd, info))
                            Log.Error("Duplicate command found in lookup table: {Command}", cmd);

                        if (!info.Hidden) {
                            string group = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                            modules[module].Add(group);
                        }
                    }
                    Log.Debug("Loaded command list from: {FileName}", fi.Name);
                } catch (JsonReaderException e) {
                    Log.Error(e, "Failed to load command list from file: {FileName}", fi.Name);
                }
            }
        }

        public string GetCommandDescription(ulong? gid, string command)
            => this.lcs.GetCommandDescription(gid, command);

        public IReadOnlyList<string> GetCommandUsageExamples(ulong? gid, string command)
        {
            CommandInfo cmdInfo = this.GetInfoForCommand(command);
            string locale = this.lcs.GetGuildLocale(gid);
            var examples = new List<string>();

            if (cmdInfo.UsageExamples.Any()) {
                foreach (List<string> args in cmdInfo.UsageExamples) {
                    string cmd = $"{this.gcs.GetGuildPrefix(gid)}{command}";
                    if (args.Any())
                        examples.Add($"{cmd} {args.Select(arg => this.lcs.GetString(gid, arg)).JoinWith(" ")}");
                    else
                        examples.Add(cmd);
                }
            }

            return examples.AsReadOnly();
        }

        public IReadOnlyList<string> GetCommandsInModule(ModuleType module)
        {
            this.AssertIsDataLoaded();
            return this.modules.GetValueOrDefault(module)?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        }

        public bool RemoveCommand(string command)
        {
            this.AssertIsDataLoaded();

            return this.commands.TryRemove(command, out CommandInfo? info)
                && info is { }
                && (this.modules.GetValueOrDefault(info.Module)?.TryRemove(command) ?? false);
        }


        private void AssertIsDataLoaded()
        {
            if (!this.isDataLoaded)
                throw new InvalidOperationException("The command data has not been loaded.");
        }

        private CommandInfo GetInfoForCommand(string command)
        {
            this.AssertIsDataLoaded();

            return this.commands.TryGetValue(command, out CommandInfo? cmdInfo)
                ? cmdInfo
                : throw new KeyNotFoundException($"Failed to find info for command: {command}");
        }


        private sealed class CommandInfo
        {
            [JsonProperty("usage")]
            public List<List<string>> UsageExamples { get; set; } = new List<List<string>>();

            [JsonProperty("hidden")]
            public bool Hidden { get; set; } = false;

            [JsonIgnore]
            public ModuleType Module { get; set; } = ModuleType.Uncategorized;
        }
    }
}
