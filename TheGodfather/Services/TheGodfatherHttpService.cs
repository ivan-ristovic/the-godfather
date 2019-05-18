#region USING_DIRECTIVES
using System.Net.Http;
#endregion

namespace TheGodfather.Services
{
    public abstract class TheGodfatherHttpService : ITheGodfatherService
    {
        protected static readonly HttpClientHandler _handler = new HttpClientHandler { AllowAutoRedirect = false };
        protected static readonly HttpClient _http = new HttpClient(_handler, true);

        public abstract bool IsDisabled { get; }
    }
}
