#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Services.Common;

using DSharpPlus;
#endregion

namespace TheGodfather.Services
{
    public class MovieInfoService : HttpService, IGodfatherService
    {
        private string _key;


        public MovieInfoService(string key)
        {
            _key = key;
        }


        public async Task<IReadOnlyList<MovieInfo>> SearchAsync(string query)
        {
            try {
                var response = await _http.GetStringAsync($"http://www.omdbapi.com/?apikey={ _key }&s={ query }")
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<OMDbResponse>(response);
                if (data.Success)
                    return data.Results.AsReadOnly();
                else
                    return null;
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }
    }
}
