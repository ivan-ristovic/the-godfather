using System.Net;
using System.Threading.Tasks;
using TheGodfather.Services;

namespace TheGodfather.Modules.Misc.Services
{
    public class InsultService : ITheGodfatherService
    {
        private readonly string endpoint = "https://insult.mattbas.org/api/en/insult.txt";

        public bool IsDisabled => false;


        public Task<string> FetchInsultAsync(string username)
            => HttpService.GetStringAsync($"{this.endpoint}?who={WebUtility.UrlEncode(username)}");
    }
}
