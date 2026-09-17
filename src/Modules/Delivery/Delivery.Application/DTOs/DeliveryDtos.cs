namespace Delivery.Application.DTOs;

public record DeliveryTrackingDto(
    Guid Id,
    Guid OrderId,
    Guid DeliveryPartnerId,
    string PartnerName,
    string PartnerPhone,
    decimal CurrentLatitude,
    decimal CurrentLongitude,
    int EtaMinutes,
    string Status);

public record RiderLocationUpdate(
    Guid OrderId,
    decimal Latitude,
    decimal Longitude,
    int EtaMinutes);

