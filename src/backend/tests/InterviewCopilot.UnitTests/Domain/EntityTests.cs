using FluentAssertions;
using InterviewCopilot.Domain.Entities;
using InterviewCopilot.Domain.Enums;
using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.UnitTests.Domain;

public class EntityTests
{
    private static readonly DateTimeOffset Now = FixedClock.Default;

    [Fact]
    public void Should_CreateSegment_When_Valid()
    {
        var segment = TranscriptSegment.Create(Guid.NewGuid(), Speaker.Interviewer, 0, 1500, " Hello there ");

        segment.Text.Should().Be("Hello there");
        segment.Speaker.Should().Be(Speaker.Interviewer);
    }

    [Theory]
    [InlineData(-1, 10, "text")]
    [InlineData(100, 50, "text")]
    [InlineData(0, 10, " ")]
    public void Should_RejectSegment_When_Invalid(long start, long end, string text)
    {
        var act = () => TranscriptSegment.Create(Guid.NewGuid(), Speaker.Candidate, start, end, text);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_CreateAnswer_When_Valid()
    {
        var answer = SuggestedAnswer.Create(Guid.NewGuid(), " Why us? ", QuestionType.Experience, "Because…", 2, AnswerModifier.Longer, Now);

        answer.Question.Should().Be("Why us?");
        answer.Version.Should().Be(2);
        answer.Modifier.Should().Be(AnswerModifier.Longer);
    }

    [Fact]
    public void Should_RejectAnswer_When_QuestionMissingOrVersionZero()
    {
        ((Action)(() => SuggestedAnswer.Create(Guid.NewGuid(), " ", QuestionType.Coding, "x", 1, AnswerModifier.None, Now)))
            .Should().Throw<DomainException>();
        ((Action)(() => SuggestedAnswer.Create(Guid.NewGuid(), "Q?", QuestionType.Coding, "x", 0, AnswerModifier.None, Now)))
            .Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Should_RejectPracticeAttempt_When_ScoreOutOfRange(int score)
    {
        var act = () => PracticeAttempt.Create(Guid.NewGuid(), "Q?", "A", score, "ok", Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_CreatePracticeAttempt_When_Valid()
    {
        var attempt = PracticeAttempt.Create(Guid.NewGuid(), " Q? ", " A ", 7, " Good ", Now);

        attempt.Score.Should().Be(7);
        attempt.Question.Should().Be("Q?");
        attempt.Feedback.Should().Be("Good");
    }

    [Fact]
    public void Should_RejectPracticeAttempt_When_QuestionMissing()
    {
        var act = () => PracticeAttempt.Create(Guid.NewGuid(), " ", "A", 5, "ok", Now);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("")]
    public void Should_RejectAccount_When_UsernameInvalid(string username)
    {
        var act = () => UserAccount.Create(username, "hash", Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_RejectAccount_When_HashMissing()
    {
        var act = () => UserAccount.Create("alice", " ", Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_ChangeHash_When_Valid_And_RejectBlank()
    {
        var account = UserAccount.Create(" alice ", "hash1", Now);

        account.ChangePasswordHash("hash2");

        account.Username.Should().Be("alice");
        account.PasswordHash.Should().Be("hash2");
        ((Action)(() => account.ChangePasswordHash(""))).Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_KeepMessages_When_ExceptionsConstructed()
    {
        new DomainException().Message.Should().NotBeNull();
        new DomainException("x", new InvalidOperationException()).InnerException.Should().NotBeNull();
    }
}
