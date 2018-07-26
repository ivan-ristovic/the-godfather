#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Collections.Generic;
using System.Xml.Serialization;

#endregion

namespace TheGodfather.Services.Common
{
    public class GoodreadsResponse
    {
        [XmlElement("search")]
        public GoodreadsSearchInfo SearchInfo { get; set; }
    }

    public class GoodreadsSearchInfo
    {
        [XmlElement("query")]
        public string Query { get; set; }

        [XmlElement("total-results")]
        public int NumberOfResults { get; set; }

        [XmlElement("query-time-seconds")]
        public float QueryTime { get; set; }

        [XmlArray("results"), XmlArrayItem("work")]
        public GoodreadsWork[] Results { get; set; }


        public IReadOnlyList<Page> ToPaginatedList()
        {
            var pages = new List<Page>();

            foreach (GoodreadsWork work in this.Results) {
                var emb = new DiscordEmbedBuilder() {
                    Title = work.Book.Title,
                    ThumbnailUrl = work.Book.ImageUrl,
                    Color = DiscordColor.Aquamarine
                };

                emb.AddField("Author", work.Book.Author.Name, inline: true);
                emb.AddField("Rating", $"{work.AverageRating} out of {work.RatingsCount} votes", inline: true);
                emb.AddField("Date", $"{work.PublicationDayString}/{work.PublicationMonthString}/{work.PublicationYearString}", inline: true);
                emb.AddField("Books count", work.BooksCount.ToString(), inline: true);
                emb.AddField("Work ID", work.Id.ToString(), inline: true);
                emb.AddField("Book ID", work.Book.Id.ToString(), inline: true);

                emb.WithFooter($"Fethed results using Goodreads API in {this.QueryTime}s");

                pages.Add(new Page() { Embed = emb.Build() });
            }

            return pages.AsReadOnly();
        }
    }

    public class GoodreadsWork
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("books_count")]
        public int BooksCount { get; set; }

        [XmlElement("ratings_count")]
        public int RatingsCount { get; set; }

        [XmlElement("text_reviews_count")]
        public int TextReviewsCount { get; set; }

        [XmlIgnore]
        private int? PublicationYear { get; set; }

        [XmlElement("original_publication_year")]
        public string PublicationYearString {
            get { return (this.PublicationYear.HasValue) ? this.PublicationYear.ToString() : null; }
            set { this.PublicationYear = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
        }

        [XmlIgnore]
        private int? PublicationMonth { get; set; }

        [XmlElement("original_publication_month")]
        public string PublicationMonthString {
            get { return (this.PublicationMonth.HasValue) ? this.PublicationMonth.ToString() : null; }
            set { this.PublicationMonth = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
        }

        [XmlIgnore]
        private int? PublicationDay { get; set; }

        [XmlElement("original_publication_day")]
        public string PublicationDayString {
            get { return (this.PublicationDay.HasValue) ? this.PublicationDay.ToString() : null; }
            set { this.PublicationDay = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
        }

        [XmlElement("average_rating")]
        public float AverageRating { get; set; }

        [XmlElement("best_book")]
        public GoodreadsBook Book { get; set; }
    }

    public class GoodreadsBook
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("author")]
        public GoodreadsAuthor Author { get; set; }

        [XmlElement("image_url")]
        public string ImageUrl { get; set; }

        [XmlElement("small_image_url")]
        public string SmallImageUrl { get; set; }
    }

    public class GoodreadsAuthor
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }
    }
}
