using FluentAssertions;
using InterviewCopilot.Domain;
using InterviewCopilot.Domain.Entities;
using InterviewCopilot.Domain.Enums;
using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.UnitTests.Domain;

public class SessionTests
{
    private static readonly DateTimeOffset Now = FixedClock.Default;

    private static Session NewSession() => Session.Create(Guid.NewGuid(), "Acme interview", SessionMode.LiveInterview, Now);

    [Fact]
    public void Should_StartAsDraftWithBalancedAnswers_When_Created()
    {
        var session = NewSession();

        session.Status.Should().Be(SessionStatus.Draft);
        session.AnswerLength.Should().Be(AnswerLength.Balanced);
        session.Framework.Should().Be(AnswerFramework.None);
        session.Title.Should().Be("Acme interview");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_TitleIsBlank(string title)
    {
        var act = () => Session.Create(Guid.NewGuid(), title, SessionMode.LiveInterview, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_Throw_When_TitleTooLong()
    {
        var act = () => Session.Create(Guid.NewGuid(), new string('x', SessionLimits.MaxTitleLength + 1), SessionMode.LiveInterview, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_AcceptTenDocuments_And_RejectTheEleventh()
    {
        var session = NewSession();
        for (var i = 0; i < SessionLimits.MaxDocumentsPerSession; i++)
        {
            session.AttachDocument($"doc{i}.txt", "content", Now);
        }

        var act = () => session.AttachDocument("one-too-many.txt", "content", Now);

        session.Documents.Should().HaveCount(10);
        act.Should().Throw<DomainException>().WithMessage("*at most 10 documents*");
    }

    [Fact]
    public void Should_RejectDocument_When_ContentIsEmpty()
    {
        var act = () => NewSession().AttachDocument("resume.txt", "  ", Now);

        act.Should().Throw<DomainException>().WithMessage("*No readable text*");
    }

    [Fact]
    public void Should_RejectDocument_When_FileNameMissingOrTooLong()
    {
        var session = NewSession();

        ((Action)(() => session.AttachDocument(" ", "text", Now))).Should().Throw<DomainException>();
        ((Action)(() => session.AttachDocument(new string('a', 256), "text", Now))).Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_RemoveDocument_When_Attached()
    {
        var session = NewSession();
        var document = session.AttachDocument("resume.txt", "text", Now);

        session.RemoveDocument(document.Id);

        session.Documents.Should().BeEmpty();
    }

    [Fact]
    public void Should_Throw_When_RemovingUnknownDocument()
    {
        var act = () => NewSession().RemoveDocument(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_StoreAnswerSettings_When_Configured()
    {
        var session = NewSession();

        session.ConfigureAnswers(AnswerLength.Short, AnswerFramework.Star, QuestionType.Behavioral, "  Panel of engineers  ");

        session.AnswerLength.Should().Be(AnswerLength.Short);
        session.Framework.Should().Be(AnswerFramework.Star);
        session.PinnedQuestionType.Should().Be(QuestionType.Behavioral);
        session.SessionPrompt.Should().Be("Panel of engineers");
    }

    [Fact]
    public void Should_Throw_When_SessionPromptTooLong()
    {
        var act = () => NewSession().ConfigureAnswers(
            AnswerLength.Short, AnswerFramework.None, null, new string('x', SessionLimits.MaxSessionPromptLength + 1));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Should_MoveThroughLifecycle_When_StartedThenEnded()
    {
        var session = NewSession();

        session.Start(Now);
        session.End(Now.AddMinutes(30));

        session.Status.Should().Be(SessionStatus.Ended);
        session.StartedAt.Should().Be(Now);
        session.EndedAt.Should().Be(Now.AddMinutes(30));
    }

    [Fact]
    public void Should_Throw_When_StartingTwiceOrEndingDraft()
    {
        var session = NewSession();

        ((Action)(() => session.End(Now))).Should().Throw<DomainException>();
        session.Start(Now);
        ((Action)(() => session.Start(Now))).Should().Throw<DomainException>();
    }
}
