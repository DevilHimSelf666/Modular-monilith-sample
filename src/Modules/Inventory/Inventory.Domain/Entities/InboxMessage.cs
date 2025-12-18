namespace Inventory.Domain.Entities;

public class InboxMessage
{
    private InboxMessage()
    {
    }

    public InboxMessage(Guid messageId)
    {
        MessageId = messageId;
        ReceivedAtUtc = DateTime.UtcNow;
    }

    public Guid MessageId { get; private set; }

    public DateTime ReceivedAtUtc { get; private set; }

    public DateTime? ProcessedAtUtc { get; private set; }

    public void MarkProcessed()
    {
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
