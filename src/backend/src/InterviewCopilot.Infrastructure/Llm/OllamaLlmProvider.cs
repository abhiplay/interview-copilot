using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using InterviewCopilot.Application.Providers;

namespace InterviewCopilot.Infrastructure.Llm;

/// <summary>Local, free LLM via Ollama's streaming chat API (default provider, NFR-01).</summary>
internal sealed class OllamaLlmProvider(HttpClient http) : ILlmProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string Name => ProviderNames.Ollama;

    public IAsyncEnumerable<string> StreamAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return StreamCoreAsync(request, cancellationToken);
    }

    private async IAsyncEnumerable<string> StreamCoreAsync(
        LlmRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var body = new OllamaChatRequest(
            request.Model,
            [.. request.Messages.Select(m => new OllamaMessage(RoleName(m.Role), m.Content))],
            Stream: true,
            new OllamaRequestOptions(request.Temperature));

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/chat")
        {
            Content = JsonContent.Create(body, options: JsonOptions),
        };

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new LlmUnavailableException(
                "Can't reach Ollama. Make sure it's installed and running (https://ollama.com).", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new LlmUnavailableException(
                    $"Ollama returned {(int)response.StatusCode}. Is the model '{request.Model}' pulled? Run: ollama pull {request.Model}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            while (await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var chunk = JsonSerializer.Deserialize<OllamaChatChunk>(line, JsonOptions);
                if (!string.IsNullOrEmpty(chunk?.Message?.Content))
                {
                    yield return chunk.Message.Content;
                }

                if (chunk?.Done == true)
                {
                    yield break;
                }
            }
        }
    }

    private static string RoleName(LlmRole role) => role switch
    {
        LlmRole.System => "system",
        LlmRole.Assistant => "assistant",
        _ => "user",
    };

    private sealed record OllamaChatRequest(
        string Model,
        IReadOnlyList<OllamaMessage> Messages,
        bool Stream,
        OllamaRequestOptions Options);

    private sealed record OllamaMessage(string Role, string Content);

    private sealed record OllamaRequestOptions([property: JsonPropertyName("temperature")] double Temperature);

    private sealed record OllamaChatChunk(OllamaMessage? Message, bool Done);
}
