namespace Procurement.Contracts;

public sealed record ProcurementRequestDto(Guid Id, string InquiryType, decimal Amount, string WorkflowInstanceId);
