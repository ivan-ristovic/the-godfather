#nullable disable
using System.Xml.Serialization;

namespace TheGodfather.Modules.Search.Common
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
            get => this.PublicationYear?.ToString();
            set { this.PublicationYear = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
        }

        [XmlIgnore]
        private int? PublicationMonth { get; set; }

        [XmlElement("original_publication_month")]
        public string PublicationMonthString {
            get => this.PublicationMonth?.ToString();
            set { this.PublicationMonth = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
        }

        [XmlIgnore]
        private int? PublicationDay { get; set; }

        [XmlElement("original_publication_day")]
        public string PublicationDayString {
            get => this.PublicationDay?.ToString();
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
