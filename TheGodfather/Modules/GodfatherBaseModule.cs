using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace TheGodfather.Modules
{
    public abstract class GodfatherBaseModule : BaseExtension
    {
        protected SharedData SharedData;
        protected DatabaseService DatabaseService;


        protected GodfatherBaseModule(SharedData shared, DatabaseService db)
        {
            SharedData = shared;
            DatabaseService = db;
        }


        protected override void Setup(DiscordClient client)
        {

        }


        protected async Task ReplySuccessAsync(CommandContext ctx, string msg = "Done!")
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{DiscordEmoji.FromName(ctx.Client, ":white_check_mark:")} {msg}",
                Color = DiscordColor.Green
            }).ConfigureAwait(false);
        }

        protected string GetReasonString(CommandContext ctx, string reason = null)
            => $"{ctx.User.ToString()} : {reason ?? "No reason provided."} | Invoked in: {ctx.Channel.ToString()}";

    }
}
