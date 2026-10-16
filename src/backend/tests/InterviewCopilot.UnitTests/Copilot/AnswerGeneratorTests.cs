using FluentAssertions;
using InterviewCopilot.Application.Common.Exceptions;
using InterviewCopilot.Application.Copilot;
using InterviewCopilot.Application.Providers;
using InterviewCopilot.Domain.Entities;
using InterviewCopilot.Domain.Enums;
using Microsoft.Extensions.Options;

namespace InterviewCopilot.UnitTests.Copilot;

public class AnswerGeneratorTests
{
    private readonly InMemoryStore _store = new();
    private readonly Session _session = Session.Create(Guid.NewGuid(), "Interview", SessionMode.LiveInterview, FixedClock.Default);

    public AnswerGeneratorTests() => _store.Add(_session);

    private AnswerGenerator Generator(FakeLlm llm) =>
        new(_store, _store, _store, llm, Options.Create(new LlmOptions { Model = "llama3.2:3b", Temperature = 0.2 }), new FixedClock());

    [Fact]
    public async Task Should_StreamTokens_And_PersistAnswer()
    {
        var llm = new FakeLlm("I ", "led ", "it.");
        var (header, tokens) = await Generator(llm).GenerateAsync(
            new AnswerRequest(_session.Id, "Tell me about a time you led a team", ApiKey: "byok"),
            TestContext.Current.CancellationToken);

        var streamed = new List<string>();
        await foreach (var token in tokens)
        {
            streamed.Add(token);
        }

        header.QuestionType.Should().Be(QuestionType.Behavioral);
        header.Version.Should().Be(1);
        string.Concat(streamed).Should().Be("I led it.");
        _store.Answers.Should().ContainSingle().Which.Text.Should().Be("I led it.");
        _store.SaveCount.Should().Be(1);
        llm.LastRequest!.Model.Should().Be("llama3.2:3b");
        llm.LastRequest.ApiKey.Should().Be("byok");
    }

    [Fact]
    public async Task Should_IncrementVersion_When_Regenerating()
    {
        var (header, _) = await Generator(new FakeLlm("x")).GenerateAsync(
            new AnswerRequest(_session.Id, "Why this company?", AnswerModifier.Regenerate, "old", PreviousVersion: 2),
            TestContext.Current.CancellationToken);

        header.Version.Should().Be(3);
    }

    [Fact]
    public async Task Should_Throw_When_SessionMissing()
    {
        var act = () => Generator(new FakeLlm()).GenerateAsync(new AnswerRequest(Guid.NewGuid(), "Q?"), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
