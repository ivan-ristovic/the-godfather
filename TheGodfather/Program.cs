

namespace TheGodfatherBot
{
    class Program
    {
        public static void Main(string[] args) =>
            new TheGodfather().MainAsync(args).GetAwaiter().GetResult();
    }
}
