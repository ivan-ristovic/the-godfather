using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using TheGodfather.Common;
using TheGodfather.Extensions;

namespace TheGodfather.Modules.Misc.Services
{
    public static class SimulationService
    {
        private static readonly SecureRandom _rng = new SecureRandom();
        private static readonly Regex _urlRegex =
            new Regex(@"([a-z]+:\/*)?[-a-z0-9@:%._\+~#=]{1,256}\.[a-z0-9()]{1,6}\b([-a-z0-9()@:%_\+.~#?&\/=]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public static async Task<string?> SimulateAsync(DiscordChannel channel, DiscordMember member, string ignorePrefix)
        {
            var pastMessages = new List<DiscordMessage>();
            int count = 25;
            do {
                IReadOnlyList<DiscordMessage> chunk = await channel.GetMessagesAsync(count);
                pastMessages.AddRange(
                    chunk
                        .Where(m => m.Author.Id == member.Id && !string.IsNullOrWhiteSpace(m.Content))
                        .Where(m => !m.Content.StartsWith(ignorePrefix))
                );
            } while (pastMessages.Count < 2 && count < 500);

            if (pastMessages.Count == 0)
                return null;

            var words = new List<string>();
            foreach (DiscordMessage message in pastMessages) {
                IEnumerable<string> split = SplitMessage(message.Content);
                if (!split.Any())
                    continue;
                words.Add(split.JoinWith(" "));
            }

            return words.Count > 0 ? words.Shuffle(_rng).JoinWith(" ") : null;


            static IEnumerable<string> SplitMessage(string content)
            {
                string sanitizedContent = _urlRegex.Replace(Formatter.Strip(content), string.Empty);
                if (string.IsNullOrWhiteSpace(sanitizedContent))
                    return Enumerable.Empty<string>();

                var words = sanitizedContent
                    .Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries)
                    .Shuffle(_rng)
                    .Distinct()
                    .ToList();
                if (words.Count < 3)
                    return words;

                int start = _rng.Next(words.Count);
                int count = _rng.Next(0, words.Count - start);
                return words.Skip(start).Take(count);
            }
        }
    }
}
