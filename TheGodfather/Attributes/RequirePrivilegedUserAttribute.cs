using Discord.Commands;
using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequirePrivilegedUserAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if (await IsOwnerAsync(context))
            return PreconditionResult.FromSuccess();

        PrivilegedUserService privileged = services.GetRequiredService<PrivilegedUserService>();
        if (await privileged.ContainsAsync(context.User.Id))
            return PreconditionResult.FromSuccess();

        LocalizationService localization = services.GetRequiredService<LocalizationService>();
        string reason = localization.GetString(context.Guild?.Id, TranslationKey.cmd_chk_perms_priv);
        return PreconditionResult.FromError(reason);
    }

    private static async Task<bool> IsOwnerAsync(ICommandContext context)
    {
        IApplication app = await context.Client.GetApplicationInfoAsync();
        return app.Owner?.Id == context.User.Id;
    }
}
