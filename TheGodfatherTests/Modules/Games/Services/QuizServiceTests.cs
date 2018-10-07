#region USING_DIRECTIVES
using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Services;
#endregion

namespace TheGodfatherTests.Modules.Games.Services
{
    [TestFixture]
    public class QuizServiceTests
    {
        [Test]
        public async Task GetCategoryIdAsyncTest()
        {
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("art"));
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("vehicles"));
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("vehicle"));
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("sport"));
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("sports"));
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("history"));
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("science computers"));
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("science mathematics"));
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("enternainment film"));
            Assert.IsNotNull(await QuizService.GetCategoryIdAsync("enternainment books"));

            Assert.IsNull(await QuizService.GetCategoryIdAsync("test test test fail"));
            
            Assert.ThrowsAsync(typeof(ArgumentException), () => QuizService.GetCategoryIdAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => QuizService.GetCategoryIdAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => QuizService.GetCategoryIdAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => QuizService.GetCategoryIdAsync("\n"));
        }

        [Test]
        public async Task GetCategoriesAsyncTest()
        {
            IReadOnlyList<QuizCategory> result = await QuizService.GetCategoriesAsync();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            CollectionAssert.AllItemsAreNotNull(result);
            CollectionAssert.AllItemsAreUnique(result);
        }

        [Test]
        public async Task GetQuestionsAsyncTest()
        {
            IReadOnlyList<QuizQuestion> questions;

            questions = await QuizService.GetQuestionsAsync(9);
            Assert.IsNotNull(questions);
            Assert.AreEqual(10, questions.Count);
            CollectionAssert.AllItemsAreNotNull(questions);
            CollectionAssert.AllItemsAreUnique(questions);

            questions = await QuizService.GetQuestionsAsync(9, 5);
            Assert.IsNotNull(questions);
            Assert.AreEqual(5, questions.Count);
            CollectionAssert.AllItemsAreNotNull(questions);
            CollectionAssert.AllItemsAreUnique(questions);

            questions = await QuizService.GetQuestionsAsync(9, 5, QuestionDifficulty.Hard);
            Assert.IsNotNull(questions);
            Assert.AreEqual(5, questions.Count);
            CollectionAssert.AllItemsAreNotNull(questions);
            CollectionAssert.AllItemsAreUnique(questions);

            Assert.IsNull(await QuizService.GetQuestionsAsync(50000));

            Assert.ThrowsAsync(typeof(ArgumentException), () => QuizService.GetQuestionsAsync(-1));
            Assert.ThrowsAsync(typeof(ArgumentException), () => QuizService.GetQuestionsAsync(9, -1));
            Assert.ThrowsAsync(typeof(ArgumentException), () => QuizService.GetQuestionsAsync(9, 0));
            Assert.ThrowsAsync(typeof(ArgumentException), () => QuizService.GetQuestionsAsync(9, 100));
            Assert.ThrowsAsync(typeof(ArgumentException), () => QuizService.GetQuestionsAsync(9, 21));
        }
    }
}
