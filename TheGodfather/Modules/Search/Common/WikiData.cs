#region USING_DIRECTIVES
using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.Modules.Search.Common
{
    public class WikiSearchResponse : IReadOnlyList<WikiSearchResult>
    {
        private readonly string query;
        private readonly IReadOnlyList<string> hits;
        private readonly IReadOnlyList<string> snippets;
        private readonly IReadOnlyList<string> urls;


        public WikiSearchResponse(string query, IReadOnlyList<string> hits, IReadOnlyList<string> snippets, IReadOnlyList<string> urls)
        {
            this.query = query;
            this.hits = hits;
            this.snippets = snippets;
            this.urls = urls;
        }


        public int Count => this.hits.Count;

        public IEnumerator<WikiSearchResult> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public WikiSearchResult this[int index] => new WikiSearchResult(this.hits[index], this.snippets[index], this.urls[index]);
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
