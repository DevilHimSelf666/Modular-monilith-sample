namespace Inventory.Contracts;

public sealed record ReservationDto(Guid ProcurementRequestId, string InquiryType, decimal Amount, DateTime CreatedAtUtc);
