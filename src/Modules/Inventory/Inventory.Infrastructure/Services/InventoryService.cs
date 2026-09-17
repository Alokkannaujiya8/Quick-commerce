namespace Inventory.Infrastructure.Services;

using Inventory.Application.DTOs;
using Inventory.Application.Services;
using Inventory.Infrastructure.Persistence;

public class InventoryService : IInventoryService
{
    private readonly InventoryDbContext _context;

    public InventoryService(InventoryDbContext context)
    {
        _context = context;
    }

    public Task<IReadOnlyList<DarkStoreDto>> GetActiveDarkStoresAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DarkStoreDto> sampleStores = new List<DarkStoreDto>
        {
            new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Indiranagar Dark Store #04", "BLR-INDIRA-04", 12.9716m, 77.6412m, 4.5, true),
            new(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Koramangala Dark Store #12", "BLR-KORAM-12", 12.9352m, 77.6245m, 4.0, true),
            new(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "HSR Layout Dark Store #07", "BLR-HSR-07", 12.9121m, 77.6446m, 5.0, true)
        };

        return Task.FromResult(sampleStores);
    }

    public async Task<ServiceabilityResponse> CheckServiceabilityAsync(ServiceabilityRequest request, CancellationToken cancellationToken = default)
    {
        var stores = await GetActiveDarkStoresAsync(cancellationToken);
        DarkStoreDto? nearest = null;
        double minDistance = double.MaxValue;

        foreach (var store in stores)
        {
            var dist = CalculateHaversineDistance((double)request.Latitude, (double)request.Longitude, (double)store.Latitude, (double)store.Longitude);
            if (dist <= store.ServiceRadiusKm && dist < minDistance)
            {
                minDistance = dist;
                nearest = store;
            }
        }

        if (nearest is not null)
        {
            var eta = Math.Max(7, (int)Math.Ceiling(minDistance * 2.5) + 3);
            return new ServiceabilityResponse(true, nearest, Math.Round(minDistance, 2), eta);
        }

        return new ServiceabilityResponse(false, null, 0, 0);
    }

    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371; // Earth radius in km
        var dLat = (lat2 - lat1) * (Math.PI / 180.0);
        var dLon = (lon2 - lon1) * (Math.PI / 180.0);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * (Math.PI / 180.0)) * Math.Cos(lat2 * (Math.PI / 180.0)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }
}

