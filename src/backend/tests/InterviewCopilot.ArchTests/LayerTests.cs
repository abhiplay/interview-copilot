using FluentAssertions;
using InterviewCopilot.Application;
using InterviewCopilot.Domain.Entities;
using NetArchTest.Rules;

namespace InterviewCopilot.ArchTests;

/// <summary>Clean Architecture dependency rules (context/architecture.md). Keeps Mode C possible.</summary>
public class LayerTests
{
    private const string Application = "InterviewCopilot.Application";
    private const string Infrastructure = "InterviewCopilot.Infrastructure";
    private const string Api = "InterviewCopilot.Api";

    [Fact]
    public void Domain_Should_DependOnNothingInternal()
    {
        var result = Types.InAssembly(typeof(Session).Assembly)
            .ShouldNot().HaveDependencyOnAny(Application, Infrastructure, Api, "Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_Should_NotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(typeof(DependencyInjection).Assembly)
            .ShouldNot().HaveDependencyOnAny(Infrastructure, Api, "Microsoft.EntityFrameworkCore", "Whisper.net", "QuestPDF")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_Should_NotDependOnApi()
    {
        var result = Types.InAssembly(typeof(Infrastructure.DependencyInjection).Assembly)
            .ShouldNot().HaveDependencyOn(Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void ProviderAdapters_Should_BeInternal()
    {
        var result = Types.InAssembly(typeof(Infrastructure.DependencyInjection).Assembly)
            .That().HaveNameEndingWith("Provider")
            .Should().NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }
}
