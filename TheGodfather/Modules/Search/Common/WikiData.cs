#region USING_DIRECTIVES
using Newtonsoft.Json;

using System;
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.Modules.Search.Common
{
    public class WikiSearchResponse
    {
        public string Query { get; set; }
        public string[] Hits { get; set; }
        public string[] Snippets { get; set; }
        public string[] Urls { get; set; }


        public WikiSearchResult GetResult(int id) => new WikiSearchResult(this.Hits[id], this.Snippets[id], this.Urls[id]);
    }

    public class WikiSearchResult
    {
        public string Title { get; }
        public string Snippet { get; }
        public string Url { get; }


        public WikiSearchResult(string title, string snippet, string url)
        {
            this.Title = title;
            this.Snippet = snippet;
            this.Url = url;
        }
    }
}
