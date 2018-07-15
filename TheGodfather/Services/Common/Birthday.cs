#region USING_DIRECTIVES
using System;
#endregion

namespace TheGodfather.Services.Common
{
    public class Birthday
    {
        public ulong ChannelId { get; set; }
        public DateTime Date { get; set; }
        public ulong UserId { get; set; }
    }
}
