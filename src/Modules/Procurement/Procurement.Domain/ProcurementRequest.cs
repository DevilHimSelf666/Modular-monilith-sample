namespace Procurement.Domain;

public class ProcurementRequest
{
    private ProcurementRequest()
    {
    }

    public ProcurementRequest(string inquiryType, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(inquiryType))
        {
            throw new ArgumentException("Inquiry type is required", nameof(inquiryType));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        }

        Id = Guid.NewGuid();
        InquiryType = inquiryType;
        Amount = amount;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string InquiryType { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public string WorkflowInstanceId { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public void AssignWorkflow(string workflowInstanceId)
    {
        WorkflowInstanceId = workflowInstanceId;
    }
}
