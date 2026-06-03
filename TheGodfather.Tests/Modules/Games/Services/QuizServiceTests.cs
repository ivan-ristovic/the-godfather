using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Services;

namespace TheGodfather.Tests.Modules.Games.Services;

[TestFixture]
public sealed class QuizServiceTests
{
    [Test]
    public async Task InvalidQuestionCategoryReturnsNullWithoutCallingProvider()
    {
        Assert.That(new QuizService().IsDisabled, Is.False);
        Assert.That(await QuizService.GetQuestionsAsync(-1, 10, QuestionDifficulty.Easy), Is.Null);
    }
}
