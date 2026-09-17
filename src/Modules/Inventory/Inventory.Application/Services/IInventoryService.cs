namespace Inventory.Application.Services;

using Inventory.Application.DTOs;

public interface IInventoryService
{
    Task<IReadOnlyList<DarkStoreDto>> GetActiveDarkStoresAsync(CancellationToken cancellationToken = default);
    Task<ServiceabilityResponse> CheckServiceabilityAsync(ServiceabilityRequest request, CancellationToken cancellationToken = default);
}

