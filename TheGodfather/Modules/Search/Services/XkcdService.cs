﻿#region USING_DIRECTIVES
using Newtonsoft.Json;

using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Services;
#endregion;

namespace TheGodfather.Modules.Search.Services
{
    public class XkcdService : TheGodfatherHttpService
    {
        public static readonly string _url = "https://xkcd.com";
        public static readonly int _comicNum = 2019;


        public static string CreateUrlForComic(int id)
        {
            if (id < 1 || id > _comicNum)
                throw new ArgumentException("Comic ID is not valid (max 2019)", nameof(id));

            return $"{_url}/{id}";
        }

        public static Task<XkcdComic> GetComicAsync(int? id = null)
        {
            if (id < 1 || id > _comicNum)
                throw new ArgumentException("Comic ID is not valid (max 2019)", nameof(id));

            return id.HasValue ? GetComicByIdAsync(id.Value) : GetLatestComicAsync();
        }

        public static Task<XkcdComic> GetRandomComicAsync()
            => GetComicByIdAsync(GFRandom.Generator.Next(_comicNum));
        

        public override bool IsDisabled() 
            => false;


        private static async Task<XkcdComic> GetComicByIdAsync(int id)
        {
            string response = await _http.GetStringAsync($"{_url}/{id}/info.0.json").ConfigureAwait(false);
            return JsonConvert.DeserializeObject<XkcdComic>(response);
        }

        private static async Task<XkcdComic> GetLatestComicAsync()
        {
            string response = await _http.GetStringAsync($"{_url}/info.0.json").ConfigureAwait(false);
            return JsonConvert.DeserializeObject<XkcdComic>(response);
        }
    }
}