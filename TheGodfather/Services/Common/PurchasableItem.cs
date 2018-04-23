namespace TheGodfather.Services.Common
{
    public class PurchasableItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ulong GuildId { get; set; }
        public int Price { get; set; }
    }
}
