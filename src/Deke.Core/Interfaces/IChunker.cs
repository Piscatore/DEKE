namespace Deke.Core.Interfaces;

public interface IChunker
{
    Task<IReadOnlyList<string>> ChunkAsync(string text, CancellationToken ct = default);
}
