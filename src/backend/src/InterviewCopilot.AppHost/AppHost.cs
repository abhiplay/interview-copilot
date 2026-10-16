// Local development orchestration only. End users never run Aspire: the desktop app starts the API itself.
// Ollama runs as the user's own install (http://127.0.0.1:11434), not as a container, to match production.
var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.InterviewCopilot_Api>("api");

builder.AddViteApp("ui", "../../../ui")
    .WithPnpm()
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync();
