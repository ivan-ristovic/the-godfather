#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Polls.Common
{
    public class ReactionsPoll : Poll
    {
        private static readonly Dictionary<string, int> _emojiid = new Dictionary<string, int>() {
            { "1\u20e3" , 0 }, { ":one:" , 0 },
            { "2\u20e3" , 1 }, { ":two:" , 1 },
            { "3\u20e3" , 2 }, { ":three:" , 2 },
            { "4\u20e3" , 3 }, { ":four:" , 3 },
            { "5\u20e3" , 4 }, { ":five:" , 4 },
            { "6\u20e3" , 5 }, { ":six:" , 5 },
            { "7\u20e3" , 6 }, { ":seven:" , 6 },
            { "8\u20e3" , 7 }, { ":eight:" , 7 },
            { "9\u20e3" , 8 }, { ":nine:" , 8 },
            { "\U0001f51f" , 9 }, { ":keycap_ten:" , 9 },
        };


        private DiscordMessage _message;
        private ReactionCollectionContext _result;


        public ReactionsPoll(InteractivityExtension interactivity, DiscordChannel channel, string question)
            : base(interactivity, channel, question) { }


        public override async Task RunAsync(TimeSpan timespan)
        {
            this._endTime = DateTime.Now + timespan;
            this.Running = true;
            this._message = await this._channel.SendMessageAsync(embed: EmbedPoll())
                .ConfigureAwait(false);
            this._result = await this._interactivity.CreatePollAsync(this._message, StaticDiscordEmoji.Numbers.Take(this.OptionCount), timespan)
                .ConfigureAwait(false);
            await this._channel.SendMessageAsync(embed: EmbedPollResults())
                .ConfigureAwait(false);
            this.Running = false;
        }

        public override DiscordEmbed EmbedPoll()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = Question,
                Description = "Vote by clicking on the reactions!",
                Color = DiscordColor.Orange
            };
            for (int i = 0; i < this._options.Count; i++)
                if (!string.IsNullOrWhiteSpace(this._options[i]))
                    emb.AddField($"{i + 1}", this._options[i], inline: true);

            if (this._endTime != null)
                emb.WithFooter($"Poll ends at: {this._endTime.ToUniversalTime().ToString()} UTC (in {this.UntilEnd:hh\\:mm\\:ss})");

            return emb.Build();
        }

        public override DiscordEmbed EmbedPollResults()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = this.Question + " (results)",
                Color = DiscordColor.Orange
            };

            if (!this._result.Reactions.Any())
                return emb.WithDescription("Nobody voted!").Build();

            foreach (var kvp in this._result.Reactions) 
                emb.AddField(this._options[_emojiid[kvp.Key.Name]], kvp.Value.ToString(), inline: true);

            return emb.Build();
        }
    }
}
