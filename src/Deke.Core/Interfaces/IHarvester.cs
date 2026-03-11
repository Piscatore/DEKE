using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IHarvester
{
    SourceType SupportedType { get; }
    Task<HarvestResult> HarvestAsync(Source source, CancellationToken ct = default);
}
