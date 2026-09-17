namespace Delivery.Infrastructure.Services;

using Delivery.Application.DTOs;
using Delivery.Application.Services;
using Delivery.Infrastructure.Persistence;

public class DeliveryService : IDeliveryService
{
    private readonly DeliveryDbContext _context;

    public DeliveryService(DeliveryDbContext context)
    {
        _context = context;
    }

    public Task<DeliveryTrackingDto> AssignRiderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var sampleRider = new DeliveryTrackingDto(
            Guid.NewGuid(),
            orderId,
            Guid.NewGuid(),
            "Ramesh Kumar",
            "+919876543219",
            12.9716m,
            77.6412m,
            8,
            "OutForDelivery"
        );

        return Task.FromResult(sampleRider);
    }

    public Task<DeliveryTrackingDto?> GetTrackingByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var tracking = new DeliveryTrackingDto(
            Guid.NewGuid(),
            orderId,
            Guid.NewGuid(),
            "Ramesh Kumar",
            "+919876543219",
            12.9720m,
            77.6420m,
            6,
            "OutForDelivery"
        );

        return Task.FromResult<DeliveryTrackingDto?>(tracking);
    }

    public Task UpdateRiderLocationAsync(RiderLocationUpdate update, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

