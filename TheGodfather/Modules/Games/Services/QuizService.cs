using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games.Services
{
    public sealed class QuizService : TheGodfatherHttpService
    {
        public const string ApiUrl = "https://opentdb.com";

        public override bool IsDisabled => false;


        public static async Task<int?> GetCategoryIdAsync(string category)
        {
            category = category.ToLowerInvariant();

            IReadOnlyList<QuizCategory>? categories = await GetCategoriesAsync().ConfigureAwait(false);
            QuizCategory? result = categories
                ?.OrderBy(c => category.LevenshteinDistanceTo(c.Name.ToLowerInvariant()))
                ?.FirstOrDefault();

            if (result is null || category.LevenshteinDistanceTo(result.Name.ToLowerInvariant()) > 2)
                return null;

            return result.Id;
        }

        public static async Task<IReadOnlyList<QuizCategory>?> GetCategoriesAsync()
        {
            try {
                string response = await _http.GetStringAsync($"{ApiUrl}/api_category.php").ConfigureAwait(false);
                QuizCategoryList data = JsonConvert.DeserializeObject<QuizCategoryList>(response) ?? throw new JsonSerializationException();
                return data.Categories.AsReadOnly();
            } catch (Exception e) {
                Log.Error(e, "Failed to fetch quiz categories");
                return null;
            }
        }

        public static async Task<IReadOnlyList<QuizQuestion>?> GetQuestionsAsync(int category, int amount, QuestionDifficulty difficulty)
        {
            if (category < 0)
                return null;

            if (amount is < 1 or > 20)
                amount = 10;

            QuizData? data = null;
            string url = $"{ApiUrl}/api.php?amount={amount}&category={category}&difficulty={difficulty.ToString().ToLower()}&type=multiple&encode=url3986";
            try {
                string response = await _http.GetStringAsync(url).ConfigureAwait(false);
                data = JsonConvert.DeserializeObject<QuizData>(response);
            } catch (Exception e) {
                Log.Error(e, "Failed to fetch {QuizQuestionAmount} quiz questions from category {QuizCategoryId}", amount, category);
            }

            if (data?.ResponseCode == 0) {
                IEnumerable<QuizQuestion> questions = data.Questions.Select(q => {
                    q.Content = WebUtility.UrlDecode(q.Content);
                    q.Category = WebUtility.UrlDecode(q.Category);
                    q.CorrectAnswer = WebUtility.UrlDecode(q.CorrectAnswer);
                    q.Difficulty = difficulty;
                    q.IncorrectAnswers = q.IncorrectAnswers.Select(ans => WebUtility.UrlDecode(ans)).ToList();
                    return q;
                });
                return questions.ToList().AsReadOnly();
            } else {
                return null;
            }
        }

    }
}
