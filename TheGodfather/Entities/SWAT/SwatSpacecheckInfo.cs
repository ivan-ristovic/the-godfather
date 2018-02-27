using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace TheGodfather.Entities.SWAT
{
    public sealed class SwatSpacecheckInfo
    {
        public DiscordUser UserId { get; }
        public CommandContext Context { get; }
    }
}
