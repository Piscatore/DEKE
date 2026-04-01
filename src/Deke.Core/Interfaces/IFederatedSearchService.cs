using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IFederatedSearchService
{
    Task<SearchResponse> SearchAsync(
        FederatedSearchRequest request,
        FederationContext? federation = null,
        CancellationToken ct = default);

    Task<ContextResponse> GetContextAsync(
        FederatedContextRequest request,
        FederationContext? federation = null,
        CancellationToken ct = default);
}
