namespace TheGodfather.Modules.Misc.Common
{
    public class PurchasableItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ulong GuildId { get; set; }
        public long Price { get; set; }
    }
}
