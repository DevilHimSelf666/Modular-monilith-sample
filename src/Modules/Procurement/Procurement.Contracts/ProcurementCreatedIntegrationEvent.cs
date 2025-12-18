namespace Procurement.Contracts;

public sealed record ProcurementCreatedIntegrationEvent(
    Guid MessageId,
    Guid CorrelationId,
    Guid ProcurementRequestId,
    string InquiryType,
    decimal Amount,
    string WorkflowInstanceId,
    DateTime OccurredAtUtc,
    int Version);
