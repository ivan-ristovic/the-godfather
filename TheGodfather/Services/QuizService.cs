﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Services.Common;

using DSharpPlus;
using System.Net;
#endregion

namespace TheGodfather.Services
{
    public class QuizService : HttpService
    {
        public static async Task<IReadOnlyList<QuizCategory>> GetQuizCategoriesAsync()
        {
            try {
               var response = await _http.GetStringAsync("https://opentdb.com/api_category.php")
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<QuizCategoryList>(response);
                return data.Categories.AsReadOnly();
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return new List<QuizCategory>();
            }
        }

        public static async Task<IReadOnlyList<QuizQuestion>> GetQuizQuestionsAsync(int category, int amount = 10, QuestionDifficulty difficulty = QuestionDifficulty.Easy, QuestionType type = QuestionType.MultipleChoice)
        {
            try {
                var url = $"https://opentdb.com/api.php?amount={ amount }&category={ category }&difficulty={ difficulty.ToAPIString() }&type={ type.ToAPIString() }&encode=url3986";
                var response = await _http.GetStringAsync(url)
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<QuizData>(response);
                if (data.ResponseCode == 0)
                    return data.Questions.Select(q => {
                        q.Content = WebUtility.UrlDecode(q.Content);
                        q.Category = WebUtility.UrlDecode(q.Category);
                        q.CorrectAnswer = WebUtility.UrlDecode(q.CorrectAnswer);
                        q.Type = type;
                        q.Difficulty = difficulty;
                        q.IncorrectAnswers = q.IncorrectAnswers.Select(ans => WebUtility.UrlDecode(ans)).ToList();
                        return q;
                    }).ToList().AsReadOnly();
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
            }

            return null;
        }
    }
}
