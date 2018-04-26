using System.Net.Http;

namespace TheGodfather.Services
{
    public abstract class TheGodfatherHttpService : IGodfatherService
    {
        protected static HttpClientHandler _handler = new HttpClientHandler { AllowAutoRedirect = false };
        protected static HttpClient _http { get; } = new HttpClient(_handler, true);
    }
}
