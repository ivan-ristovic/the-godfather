#region USING_DIRECTIVES
using Newtonsoft.Json;
using System.Collections.Generic;
#endregion

namespace TheGodfather.Services.Common
{
    public enum QuestionDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public static class QuestionDifficultyExtensions
    {
        public static string ToAPIString(this QuestionDifficulty diff)
        {
            switch (diff) {
                case QuestionDifficulty.Easy: return "easy";
                case QuestionDifficulty.Medium: return "medium";
                case QuestionDifficulty.Hard: return "hard";
                default: return "unknown";
            }
        }
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
}
