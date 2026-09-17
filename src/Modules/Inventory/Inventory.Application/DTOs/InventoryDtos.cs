namespace Inventory.Application.DTOs;

public record DarkStoreDto(
    Guid Id,
    string Name,
    string Code,
    decimal Latitude,
    decimal Longitude,
    double ServiceRadiusKm,
    bool IsActive);

public record ServiceabilityRequest(
    decimal Latitude,
    decimal Longitude);

public record ServiceabilityResponse(
    bool IsServiceable,
    DarkStoreDto? NearestStore,
    double DistanceKm,
    int EstimatedDeliveryMinutes);

