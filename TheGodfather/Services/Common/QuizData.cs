using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TheGodfather.Services.Common
{
    public enum QuestionDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum QuestionType
    {
        TrueFalse,
        MultipleChoice
    }

    public static class QuizEnumDataExtensions
    {
        public static string ToAPIString(this QuestionDifficulty diff)
        {
            switch (diff) {
                case QuestionDifficulty.Easy: return "easy";
                case QuestionDifficulty.Medium: return "medium";
                case QuestionDifficulty.Hard: return "hard";
            }
            return "unknown";
        }

        public static string ToAPIString(this QuestionType type)
        {
            switch (type) {
                case QuestionType.MultipleChoice: return "multiple";
                case QuestionType.TrueFalse: return "boolean";
            }
            return "unknown";
        }
    }


    public class QuizData
    {
        [JsonProperty("response_code")]
        public int ResponseCode { get; set; }

        [JsonProperty("results")]
        public List<QuizQuestion> Questions { get; set; }
    }

    public class QuizQuestion
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonIgnore]
        public QuestionType Type { get; set; }

        [JsonIgnore]
        public QuestionDifficulty Difficulty { get; set; }

        [JsonProperty("question")]
        public string Content { get; set; }

        [JsonProperty("correct_answer")]
        public string CorrectAnswer { get; set; }

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
        [JsonProperty("response_code")]
        public int ResponseCode { get; set; }

        [JsonProperty("response_message")]
        public string Message { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
