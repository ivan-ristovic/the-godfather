using System;

namespace TheGodfather.Services.Common
{
    public class Birthday
    {
        public ulong UserId { get; }
        public ulong ChannelId { get; }
        public DateTime Date { get; }


        public Birthday(ulong uid, ulong cid, DateTime? date = null)
        {
            UserId = uid;
            ChannelId = cid;
            Date = date ?? DateTime.UtcNow.Date;
        }
    }
}
