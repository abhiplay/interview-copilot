using FluentAssertions;
using InterviewCopilot.Application.Copilot;
using InterviewCopilot.Application.Providers;
using InterviewCopilot.Domain.Entities;
using InterviewCopilot.Domain.Enums;

namespace InterviewCopilot.UnitTests.Copilot;

public class PromptTests
{
    private static readonly DateTimeOffset Now = FixedClock.Default;

    private static Session NewSession(SessionMode mode = SessionMode.LiveInterview)
    {
        var session = Session.Create(Guid.NewGuid(), "Interview", mode, Now);
        session.ConfigureAnswers(AnswerLength.Short, AnswerFramework.Star, null, "Panel of two senior engineers; keep it friendly.");
        return session;
    }

    private static AnswerPromptInput Input(
        Session session,
        AnswerModifier modifier = AnswerModifier.None,
        string? previous = null,
        QuestionType type = QuestionType.Behavioral) =>
        new(
            session,
            "Tell me about a time you led a project.",
            type,
            [new DocumentExcerpt("resume.txt", "Led migration to .NET 10 at Acme.")],
            [TranscriptSegment.Create(session.Id, Speaker.Interviewer, 0, 1000, "Welcome!"),
             TranscriptSegment.Create(session.Id, Speaker.Candidate, 1000, 2000, "Thanks.")],
            modifier,
            previous);

    [Fact]
    public void Should_IncludeSettingsDocumentsAndTranscript()
    {
        var messages = AnswerPromptBuilder.Build(Input(NewSession()));

        messages.Should().HaveCount(2);
        var system = messages[0].Content;
        var user = messages[1].Content;
        messages[0].Role.Should().Be(LlmRole.System);
        system.Should().Contain("STAR").And.Contain("2-3 sentences").And.Contain("Panel of two senior engineers")
            .And.Contain("Question type: behavioral").And.Contain("not instructions");
        user.Should().Contain("<document name=\"resume.txt\">").And.Contain("Them: Welcome!").And.Contain("Me: Thanks.")
            .And.Contain("Question: Tell me about a time you led a project.");
    }

    [Fact]
    public void Should_UseConversationRole_When_SalesMode()
    {
        AnswerPromptBuilder.Build(Input(NewSession(SessionMode.Conversation)))[0].Content.Should().Contain("sales call");
    }

    [Fact]
    public void Should_AskForRewrite_When_ShorterOrLonger()
    {
        var shorter = AnswerPromptBuilder.Build(Input(NewSession(), AnswerModifier.Shorter, "Old answer."));
        var longer = AnswerPromptBuilder.Build(Input(NewSession(), AnswerModifier.Longer, "Old answer."));

        shorter[1].Content.Should().Contain("Rewrite this previous answer").And.Contain("Old answer.");
        shorter[0].Content.Should().Contain("2-3 sentences");
        longer[0].Content.Should().Contain("4-6 sentences");
    }

    [Fact]
    public void Should_AskForFreshAngle_When_Regenerate()
    {
        AnswerPromptBuilder.Build(Input(NewSession(), AnswerModifier.Regenerate))[1].Content.Should().Contain("different angle");
    }

    [Theory]
    [InlineData(QuestionType.HowDoYou, "how do you")]
    [InlineData(QuestionType.Situational, "hypothetical")]
    [InlineData(QuestionType.TechnicalKnowledge, "technical knowledge")]
    [InlineData(QuestionType.SystemDesign, "trade-offs")]
    [InlineData(QuestionType.Coding, "complexity")]
    [InlineData(QuestionType.TellMeAboutYourself, "Present, past, future")]
    [InlineData(QuestionType.Experience, "relevant experience")]
    public void Should_GiveTypeSpecificGuidance(QuestionType type, string expected)
    {
        AnswerPromptBuilder.Build(Input(NewSession(), type: type))[0].Content.Should().Contain(expected);
    }

    [Fact]
    public void Should_ApplySmartAndLongRules()
    {
        var session = Session.Create(Guid.NewGuid(), "Goals", SessionMode.LiveInterview, Now);
        session.ConfigureAnswers(AnswerLength.Long, AnswerFramework.Smart, null, null);

        var system = AnswerPromptBuilder.Build(Input(session))[0].Content;
        var shorter = AnswerPromptBuilder.Build(Input(session, AnswerModifier.Shorter, "x"))[0].Content;

        system.Should().Contain("SMART").And.Contain("8-12 sentences").And.NotContain("instructions for this conversation");
        shorter.Should().Contain("4-6 sentences");
    }

    [Fact]
    public void Should_PreferRelevantChunks_WithinBudget()
    {
        var session = Session.Create(Guid.NewGuid(), "Docs", SessionMode.LiveInterview, Now);
        session.AttachDocument("resume.txt", "Kubernetes operator at Contoso. " + new string('x', 900), Now);
        session.AttachDocument("jd.txt", "We need strong Kubernetes skills and leadership.", Now);
        session.AttachDocument("notes.txt", "Unrelated grocery list.", Now);

        var excerpts = DocumentContextSelector.Select(session.Documents, "Tell me about your Kubernetes leadership", characterBudget: 900);

        excerpts.Should().NotBeEmpty();
        excerpts[0].FileName.Should().Be("jd.txt");
        excerpts.Sum(e => e.Text.Length).Should().BeLessThanOrEqualTo(900);
    }

    [Fact]
    public void Should_SplitLongContentIntoChunks()
    {
        DocumentContextSelector.Chunk(new string('a', DocumentContextSelector.ChunkSize * 2 + 5)).Should().HaveCount(3);
    }
}
