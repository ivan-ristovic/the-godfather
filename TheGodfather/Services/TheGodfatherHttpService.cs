#region USING_DIRECTIVES
using System.Net.Http;
#endregion

namespace TheGodfather.Services
{
    public abstract class TheGodfatherHttpService : ITheGodfatherService
    {
        protected static HttpClientHandler _handler = new HttpClientHandler { AllowAutoRedirect = false };
        protected static HttpClient _http = new HttpClient(_handler, true);
    }
}
