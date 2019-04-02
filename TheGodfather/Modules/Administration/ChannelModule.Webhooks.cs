#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.Models;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion


namespace TheGodfather.Modules.Administration
{
    public partial class ChannelModule : TheGodfatherModule
    {
        [Group("webhooks")]
        [Description("Manage webhooks for given channel. Group call lists all existing webhooks in channel.")]
        [Aliases("wh", "webhook", "whook")]
        [UsageExamples("!channel webhooks",
                       "!channel webhooks #general")]
        [RequirePermissions(Permissions.ManageWebhooks)]
        public class WebhooksModule : TheGodfatherModule
        {
            public WebhooksModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Turquoise;
            }


            [GroupCommand]
            public Task ExecuteGroupCommandAsync(CommandContext ctx, 
                                                [Description("Channel to list webhooks for.")] DiscordChannel channel = null)
                => this.ListAsync(ctx, channel);


            [Command("list")]
            [Description("Lists all existing webhooks in channel.")]
            [Aliases("l", "ls")]
            [UsageExamples("!channel webhooks list",
                           "!channel webhooks list #general")]
            public async Task ListAsync(CommandContext ctx,
                                       [Description("Channel to list webhooks for.")] DiscordChannel channel = null)
            {
                channel = channel ?? ctx.Channel;
                IReadOnlyList<DiscordWebhook> whs = await channel.GetWebhooksAsync();

                await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, whs.Select(wh => new Page() {
                    Embed = new DiscordEmbedBuilder() {
                        Title = $"Webhook: {wh.Name}",
                        Description = $"Token: {FormatterExtensions.Spoiler(wh.Token)}\nCreated by {wh.User.ToString()}",
                        Color = this.ModuleColor,
                        ThumbnailUrl = wh.AvatarUrl,
                        Timestamp = wh.CreationTimestamp,
                    }.Build()
                }));
            }
        }
    }
}
