using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace TheGodfather.Modules;

public abstract class TheGodfatherModule : BaseCommandModule
{
    public LocalizationService Localization { get; set; }
    public DiscordColor ModuleColor { get; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected TheGodfatherModule()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        var moduleAttr = ModuleAttribute.AttachedTo(this.GetType());
        this.ModuleColor = moduleAttr.Module.ToDiscordColor();
    }
}