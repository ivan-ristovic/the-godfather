using System;

namespace TheGodfather.Modules.Voice.Common
{
    public class SongInfo
    {
        public string Provider { get; set; }
        public string Query { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public string Thumbnail { get; set; }
        public string QueuerName { get; set; }
        public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;
        public string VideoId { get; set; }
    }
}