#nullable disable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TheGodfather.Modules.Games.Common
{
    public enum QuestionDifficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2,
    }

    public class QuizData
    {
        [JsonProperty("results")]
        public List<QuizQuestion> Questions { get; set; }

        [JsonProperty("response_code")]
        public int ResponseCode { get; set; }
    }

    public class QuizQuestion
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("question")]
        public string Content { get; set; }

        [JsonProperty("correct_answer")]
        public string CorrectAnswer { get; set; }

        [JsonIgnore]
        public QuestionDifficulty Difficulty { get; set; }

        [JsonProperty("incorrect_answers")]
        public List<string> IncorrectAnswers { get; set; }
    }

    public class QuizCategoryList
    {
        [JsonProperty("trivia_categories")]
        public List<QuizCategory> Categories { get; set; }
    }

    public class QuizCategory
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class TokenResponse
    {
        [JsonProperty("response_message")]
        public string Message { get; set; }

        [JsonProperty("response_code")]
        public int ResponseCode { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }

    public class CapitalInfo
    {
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("capital")]
        public string Capital { get; set; }
    }
}
