using FluentAssertions;
using InterviewCopilot.Application.Copilot;
using InterviewCopilot.Domain.Enums;

namespace InterviewCopilot.UnitTests.Copilot;

public class QuestionAnalysisTests
{
    [Theory]
    [InlineData("What made you apply for this role?", true)]
    [InlineData("Tell me about a time you disagreed with your manager", true)]
    [InlineData("Walk me through your last project", true)]
    [InlineData("Great, thanks for that.", false)]
    [InlineData("Okay?", false)]
    [InlineData("", false)]
    [InlineData("That's interesting. Can you explain the trade-offs there", true)]
    public void Should_DetectQuestions(string utterance, bool expected)
    {
        QuestionDetector.IsQuestion(utterance).Should().Be(expected);
    }

    [Theory]
    [InlineData("So, tell me about yourself.", QuestionType.TellMeAboutYourself)]
    [InlineData("Tell me about a time you failed.", QuestionType.Behavioral)]
    [InlineData("What would you do if a deadline slipped?", QuestionType.Situational)]
    [InlineData("How would you design a URL shortener?", QuestionType.SystemDesign)]
    [InlineData("Write a function that reverses a linked list.", QuestionType.Coding)]
    [InlineData("What's your experience with Kubernetes?", QuestionType.Experience)]
    [InlineData("How do you prioritise competing requests?", QuestionType.HowDoYou)]
    [InlineData("What is the difference between a process and a thread?", QuestionType.TechnicalKnowledge)]
    public void Should_ClassifyQuestionTypes(string question, QuestionType expected)
    {
        QuestionClassifier.Classify(question)!.Type.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Nice weather today")]
    public void Should_ReturnNull_When_NoRuleMatches(string question)
    {
        QuestionClassifier.Classify(question).Should().BeNull();
    }

    [Fact]
    public void Should_UseFallback_When_NothingPinnedOrDetected()
    {
        QuestionTypeResolver.Resolve(null, null).Should().Be(QuestionTypeResolver.Fallback);
    }

    [Fact]
    public void Should_UsePinned_When_NothingDetected()
    {
        QuestionTypeResolver.Resolve(QuestionType.Coding, null).Should().Be(QuestionType.Coding);
    }

    [Fact]
    public void Should_UseDetected_When_NothingPinned()
    {
        QuestionTypeResolver.Resolve(null, new QuestionClassification(QuestionType.Experience, 0.5))
            .Should().Be(QuestionType.Experience);
    }

    [Fact]
    public void Should_OverridePin_When_DetectionIsConfident()
    {
        QuestionTypeResolver.Resolve(QuestionType.SystemDesign, new QuestionClassification(QuestionType.Behavioral, 0.9))
            .Should().Be(QuestionType.Behavioral);
    }

    [Fact]
    public void Should_KeepPin_When_DetectionIsWeak()
    {
        QuestionTypeResolver.Resolve(QuestionType.SystemDesign, new QuestionClassification(QuestionType.HowDoYou, 0.6))
            .Should().Be(QuestionType.SystemDesign);
    }

    [Fact]
    public void Should_KeepPin_When_DetectionAgrees()
    {
        QuestionTypeResolver.Resolve(QuestionType.Coding, new QuestionClassification(QuestionType.Coding, 0.1))
            .Should().Be(QuestionType.Coding);
    }
}
