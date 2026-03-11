using Deke.Core.Interfaces;

namespace Deke.Infrastructure.Llm;

public class NoOpLlmService : ILlmService
{
    public bool IsAvailable => false;

    public Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        return Task.FromResult(string.Empty);
    }
}
