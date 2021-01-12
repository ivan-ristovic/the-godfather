using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Modules.Owner
{
    public abstract class BlockedEntityModule<T> : TheGodfatherServiceModule<BlockingService> where T : SnowflakeObject
    {
        public abstract Task<int> BlockAsync(IEnumerable<ulong> ids, string? reason);
        public abstract Task<int> UnblockAsync(IEnumerable<ulong> ids);
        public abstract Task<IReadOnlyList<(T Entity, string? Reason)>> ListBlockedAsync(CommandContext ctx);


        #region group
        public Task BaseExecuteGroupAsync(CommandContext ctx)
            => this.BaseListAsync(ctx);

        public Task BaseExecuteGroupAsync(CommandContext ctx, params T[] entities)
            => this.BaseAddAsync(ctx, null, entities);

        public Task BaseExecuteGroupAsync(CommandContext ctx, string? reason, params T[] entities)
            => this.BaseAddAsync(ctx, reason, entities);

        public Task BaseExecuteGroupAsync(CommandContext ctx, T entity, string reason)
            => this.BaseAddAsync(ctx, reason, entity);
        #endregion

        #region add
        public Task BaseAddAsync(CommandContext ctx, params T[] entities)
            => this.BaseAddAsync(ctx, null, entities);

        public async Task BaseAddAsync(CommandContext ctx, string? reason, params T[] entities)
        {
            if (reason?.Length >= 60)
                throw new InvalidCommandUsageException(ctx, "cmd-err-rsn", BlockedEntity.ReasonLimit);

            if (entities is null || !entities.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-block-none");

            int blocked = await this.BlockAsync(entities.Select(c => c.Id), reason);
            await ctx.InfoAsync(this.ModuleColor, "fmt-block-add", blocked);

            if (typeof(T) == typeof(DiscordGuild)) {
                foreach (T entity in entities) {
                    try {
                        DiscordGuild g = await ctx.Client.GetGuildAsync(entity.Id);
                        await g.LeaveAsync();
                    } catch {
                        LogExt.Debug(ctx, "Failed to find/leave guild: {Id}", entity.Id);
                    }
                }
            }

        }

        public Task BaseAddAsync(CommandContext ctx, T entity, string reason)
            => this.BaseAddAsync(ctx, reason, entity);
        #endregion

        #region delete
        public async Task BaseDeleteAsync(CommandContext ctx, params T[] entities)
        {
            if (entities is null || !entities.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-block-none");

            int unblocked = await this.UnblockAsync(entities.Select(c => c.Id));
            await ctx.InfoAsync(this.ModuleColor, "fmt-block-del", unblocked);
        }
        #endregion

        #region list
        public async Task BaseListAsync(CommandContext ctx)
        {
            IReadOnlyList<(T Entity, string? Reason)> blocked = await this.ListBlockedAsync(ctx);
            if (!blocked.Any())
                throw new CommandFailedException(ctx, "cmd-err-block-list-none");

            await ctx.PaginateAsync(
                "str-block-list",
                blocked,
#pragma warning disable CS8603 // Possible null reference return.
                    tup => tup.Reason is { } ? $"{tup.Entity} ({tup.Reason})" : tup.Entity.ToString(),
#pragma warning restore CS8603 // Possible null reference return.
                    this.ModuleColor,
                5
            );
        }
        #endregion
    }


    [Group("blockedusers"), Module(ModuleType.Owner), Hidden]
    [Aliases("bu", "blockedu", "blockuser", "busers", "buser", "busr")]
    [RequirePrivilegedUser]
    public sealed class BlockedUsersModule : BlockedEntityModule<DiscordUser>
    {
        #region blockedusers
        [GroupCommand, Priority(3)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.BaseListAsync(ctx);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-block-add")] params DiscordUser[] entities)
            => this.BaseAddAsync(ctx, null, entities);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-rsn")] string? reason,
                                     [Description("desc-block-add")] params DiscordUser[] entities)
            => this.BaseAddAsync(ctx, reason, entities);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-block-add")] DiscordUser entity,
                                     [RemainingText, Description("desc-rsn")] string reason)
            => this.BaseAddAsync(ctx, reason, entity);
        #endregion

        #region blockedusers add
        [Command("add"), Priority(2)]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-block-add")] params DiscordUser[] entities)
            => this.BaseAddAsync(ctx, null, entities);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-rsn")] string? reason,
                            [Description("desc-block-add")] params DiscordUser[] entities)
            => this.BaseAddAsync(ctx, reason, entities);

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-block-add")] DiscordUser entity,
                            [RemainingText, Description("desc-rsn")] string reason)
            => this.BaseAddAsync(ctx, reason, entity);
        #endregion

        #region blockedusers delete
        [Command("delete")]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public Task DeleteAsync(CommandContext ctx,
                               [Description("desc-block-del")] params DiscordUser[] entities)
            => this.BaseDeleteAsync(ctx, entities);
        #endregion

        #region blockedusers list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
            => this.BaseListAsync(ctx);
        #endregion


        #region overrides
        public override Task<int> BlockAsync(IEnumerable<ulong> ids, string? reason)
            => this.Service.BlockUsersAsync(ids, reason);

        public override async Task<IReadOnlyList<(DiscordUser Entity, string? Reason)>> ListBlockedAsync(CommandContext ctx)
        {
            var validBlocked = new List<(DiscordUser, string?)>();
            var toRemove = new List<ulong>();

            foreach (BlockedUser blocked in await this.Service.GetBlockedUsersAsync()) {
                try {
                    DiscordUser usr = await ctx.Client.GetUserAsync(blocked.Id);
                    validBlocked.Add((usr, blocked.Reason));
                } catch (NotFoundException) {
                    LogExt.Warning(ctx, "Found 404 blocked user {UserId}", blocked.Id);
                    toRemove.Add(blocked.Id);
                } catch (UnauthorizedException) {
                    LogExt.Warning(ctx, "Found 403 blocked user {UserId}", blocked.Id);
                }
            }

            await this.Service.UnblockUsersAsync(toRemove);
            return validBlocked.AsReadOnly();
        }

        public override Task<int> UnblockAsync(IEnumerable<ulong> ids)
            => this.Service.UnblockUsersAsync(ids);
        #endregion

    }

    [Group("blockedchannels"), Module(ModuleType.Owner), Hidden]
    [Aliases("bc", "blockedc", "blockchannel", "bchannels", "bchannel", "bchn")]
    [RequirePrivilegedUser]
    public sealed class BlockedChannelsModule : BlockedEntityModule<DiscordChannel>
    {
        #region blockedchannels
        [GroupCommand, Priority(3)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.BaseListAsync(ctx);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-block-add")] params DiscordChannel[] entities)
            => this.BaseAddAsync(ctx, null, entities);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-rsn")] string? reason,
                                     [Description("desc-block-add")] params DiscordChannel[] entities)
            => this.BaseAddAsync(ctx, reason, entities);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-block-add")] DiscordChannel entity,
                                     [RemainingText, Description("desc-rsn")] string reason)
            => this.BaseAddAsync(ctx, reason, entity);
        #endregion

        #region blockedchannels add
        [Command("add"), Priority(2)]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-block-add")] params DiscordChannel[] entities)
            => this.BaseAddAsync(ctx, null, entities);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-rsn")] string? reason,
                            [Description("desc-block-add")] params DiscordChannel[] entities)
            => this.BaseAddAsync(ctx, reason, entities);

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-block-add")] DiscordChannel entity,
                            [RemainingText, Description("desc-rsn")] string reason)
            => this.BaseAddAsync(ctx, reason, entity);
        #endregion

        #region blockedchannels delete
        [Command("delete")]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public Task DeleteAsync(CommandContext ctx,
                               [Description("desc-block-del")] params DiscordChannel[] entities)
            => this.BaseDeleteAsync(ctx, entities);
        #endregion

        #region blockedchannels list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
            => this.BaseListAsync(ctx);
        #endregion


        #region overrides
        public override Task<int> BlockAsync(IEnumerable<ulong> ids, string? reason)
            => this.Service.BlockChannelsAsync(ids, reason);

        public override async Task<IReadOnlyList<(DiscordChannel Entity, string? Reason)>> ListBlockedAsync(CommandContext ctx)
        {
            var validBlocked = new List<(DiscordChannel, string?)>();
            var toRemove = new List<ulong>();

            foreach (BlockedChannel blocked in await this.Service.GetBlockedChannelsAsync()) {
                try {
                    DiscordChannel chn = await ctx.Client.GetChannelAsync(blocked.Id);
                    validBlocked.Add((chn, blocked.Reason));
                } catch (NotFoundException) {
                    LogExt.Warning(ctx, "Found 404 blocked channel {ChannelId}", blocked.Id);
                    toRemove.Add(blocked.Id);
                } catch (UnauthorizedException) {
                    LogExt.Warning(ctx, "Found 403 blocked channel {ChannelId}", blocked.Id);
                }
            }

            await this.Service.UnblockChannelsAsync(toRemove);
            return validBlocked;
        }

        public override Task<int> UnblockAsync(IEnumerable<ulong> ids)
            => this.Service.UnblockChannelsAsync(ids);
        #endregion
    }

    [Group("blockedguilds"), Module(ModuleType.Owner), Hidden]
    [Aliases("bg", "blockedg", "blockguild", "bguilds", "bguild", "bgld")]
    [RequirePrivilegedUser]
    public sealed class BlockedGuildsModule : BlockedEntityModule<DiscordGuild>
    {
        #region blockedguilds
        [GroupCommand, Priority(3)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.BaseListAsync(ctx);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-block-add")] params DiscordGuild[] entities)
            => this.BaseAddAsync(ctx, null, entities);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-rsn")] string? reason,
                                     [Description("desc-block-add")] params DiscordGuild[] entities)
            => this.BaseAddAsync(ctx, reason, entities);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-block-add")] DiscordGuild entity,
                                     [RemainingText, Description("desc-rsn")] string reason)
            => this.BaseAddAsync(ctx, reason, entity);
        #endregion

        #region blockedguilds add
        [Command("add"), Priority(2)]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-block-add")] params DiscordGuild[] entities)
            => this.BaseAddAsync(ctx, null, entities);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-rsn")] string? reason,
                            [Description("desc-block-add")] params DiscordGuild[] entities)
            => this.BaseAddAsync(ctx, reason, entities);

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-block-add")] DiscordGuild entity,
                            [RemainingText, Description("desc-rsn")] string reason)
            => this.BaseAddAsync(ctx, reason, entity);
        #endregion

        #region blockedguilds delete
        [Command("delete")]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public Task DeleteAsync(CommandContext ctx,
                               [Description("desc-block-del")] params DiscordGuild[] entities)
            => this.BaseDeleteAsync(ctx, entities);
        #endregion

        #region blockedguilds list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
            => this.BaseListAsync(ctx);
        #endregion


        #region overrides
        public override Task<int> BlockAsync(IEnumerable<ulong> ids, string? reason)
            => this.Service.BlockGuildsAsync(ids, reason);

        public override async Task<IReadOnlyList<(DiscordGuild Entity, string? Reason)>> ListBlockedAsync(CommandContext ctx)
        {
            var validBlocked = new List<(DiscordGuild, string?)>();
            var toRemove = new List<ulong>();

            foreach (BlockedGuild blocked in await this.Service.GetBlockedGuildsAsync()) {
                try {
                    DiscordGuild guild = await ctx.Client.GetGuildAsync(blocked.Id);
                    validBlocked.Add((guild, blocked.Reason));
                } catch (NotFoundException) {
                    LogExt.Warning(ctx, "Found 404 blocked guild {ChannelId}", blocked.Id);
                    toRemove.Add(blocked.Id);
                }
            }

            await this.Service.UnblockGuildsAsync(toRemove);
            return validBlocked;
        }

        public override Task<int> UnblockAsync(IEnumerable<ulong> ids)
            => this.Service.UnblockGuildsAsync(ids);
        #endregion
    }
}
