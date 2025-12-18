using Inventory.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Procurement.Contracts;

namespace Inventory.Infrastructure.Consumers;

public class ProcurementCreatedConsumer : IConsumer<ProcurementCreatedIntegrationEvent>
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ProcurementCreatedConsumer> _logger;

    public ProcurementCreatedConsumer(IReservationService reservationService, ILogger<ProcurementCreatedConsumer> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcurementCreatedIntegrationEvent> context)
    {
        Guid? messageId = context.MessageId ?? (Guid?)context.Message.MessageId;
        if (messageId is null || messageId == Guid.Empty)
        {
            _logger.LogWarning("Skipping message with missing MessageId");
            return;
        }

        var recorded = await _reservationService.RecordAsync(messageId.Value, context.Message, context.CancellationToken);

        if (recorded)
        {
            _logger.LogInformation(
                "Recorded procurement reservation for {RequestId} with message {MessageId}",
                context.Message.ProcurementRequestId,
                messageId);
        }
        else
        {
            _logger.LogInformation("Duplicate message {MessageId} ignored", messageId);
        }
    }
}
