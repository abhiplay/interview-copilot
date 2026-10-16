using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InterviewCopilot.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef` create migrations without starting the API:
/// dotnet ef migrations add &lt;Name&gt; --project src/InterviewCopilot.Infrastructure --output-dir Persistence/Migrations
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Design-time tooling only.")]
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InterviewCopilotDbContext>
{
    public InterviewCopilotDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<InterviewCopilotDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options);
}
