using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfatherTests.Services
{
    [TestClass]
    public class QuizServiceTests
    {
        [TestMethod]
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

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => QuizService.GetCategoryIdAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => QuizService.GetCategoryIdAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => QuizService.GetCategoryIdAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => QuizService.GetCategoryIdAsync("\n"));
        }

        [TestMethod]
        public async Task GetCategoriesAsyncTest()
        {
            IReadOnlyList<QuizCategory> result = await QuizService.GetCategoriesAsync();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            var categories = result.ToList();
            CollectionAssert.AllItemsAreNotNull(categories);
            CollectionAssert.AllItemsAreUnique(categories);
        }

        [TestMethod]
        public async Task GetQuestionsAsyncTest()
        {
            IReadOnlyList<QuizQuestion> questions;
            List<QuizQuestion> questionList;

            questions = await QuizService.GetQuestionsAsync(9);
            Assert.IsNotNull(questions);
            Assert.AreEqual(10, questions.Count);
            questionList = questions.ToList();
            CollectionAssert.AllItemsAreNotNull(questionList);
            CollectionAssert.AllItemsAreUnique(questionList);

            questions = await QuizService.GetQuestionsAsync(9, 5);
            Assert.IsNotNull(questions);
            Assert.AreEqual(5, questions.Count);
            questionList = questions.ToList();
            CollectionAssert.AllItemsAreNotNull(questionList);
            CollectionAssert.AllItemsAreUnique(questionList);

            questions = await QuizService.GetQuestionsAsync(9, 5, QuestionDifficulty.Hard);
            Assert.IsNotNull(questions);
            Assert.AreEqual(5, questions.Count);
            questionList = questions.ToList();
            CollectionAssert.AllItemsAreNotNull(questionList);
            CollectionAssert.AllItemsAreUnique(questionList);

            Assert.IsNull(await QuizService.GetQuestionsAsync(50000));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => QuizService.GetQuestionsAsync(-1));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => QuizService.GetQuestionsAsync(9, -1));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => QuizService.GetQuestionsAsync(9, 0));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => QuizService.GetQuestionsAsync(9, 100));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => QuizService.GetQuestionsAsync(9, 21));
        }
    }
}
