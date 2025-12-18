namespace Inventory.Domain.Entities;

public class ProcurementReservation
{
    private ProcurementReservation()
    {
    }

    public ProcurementReservation(Guid messageId, Guid procurementRequestId, string inquiryType, decimal amount)
    {
        Id = Guid.NewGuid();
        MessageId = messageId;
        ProcurementRequestId = procurementRequestId;
        InquiryType = inquiryType;
        Amount = amount;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid MessageId { get; private set; }

    public Guid ProcurementRequestId { get; private set; }

    public string InquiryType { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
}
