#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Modules.Messages
{
    [Group("filter", CanInvokeWithoutSubcommand = false)]
    [Description("Message filtering commands.")]
    [Aliases("f", "filters")]
    public class CommandsFilter
    {
    }
}
