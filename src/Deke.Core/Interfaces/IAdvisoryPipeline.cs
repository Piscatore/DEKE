using Deke.Core.Models;

namespace Deke.Core.Interfaces;

/// <summary>
/// Layer 2 shared core. Turns an <see cref="AdvisoryRequest"/> into a grounded,
/// cited, confidence-scored <see cref="AdvisoryResponse"/> via the 7-stage pipeline.
/// </summary>
public interface IAdvisoryPipeline
{
    Task<AdvisoryResponse> AdviseAsync(AdvisoryRequest request, CancellationToken ct = default);
}
