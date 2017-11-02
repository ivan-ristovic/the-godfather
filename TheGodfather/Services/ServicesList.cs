using TheGodfather.Helpers;

namespace TheGodfather.Services
{
    public class ServicesList
    {
        public GiphyService GiphyService { get; private set; }


        public ServicesList(BotConfig cfg)
        {
            GiphyService = new GiphyService(cfg);
        }
    }
}
