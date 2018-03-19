namespace TheGodfather.Services.Common
{
    public class Birthday
    {
        public ulong UserId { get; }
        public ulong ChannelId { get; }


        public Birthday(ulong uid, ulong cid)
        {
            UserId = uid;
            ChannelId = cid;
        }
    }
}
