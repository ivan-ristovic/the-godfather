using System.Net.Http;

using TheGodfather.Common;

namespace TheGodfather.Services
{
    public abstract class TheGodfatherHttpService : ITheGodfatherService
    {
        protected static HttpClientHandler _handler = new HttpClientHandler { AllowAutoRedirect = false };
        protected static HttpClient _http { get; } = new HttpClient(_handler, true);
    }
}
