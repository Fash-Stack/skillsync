namespace SkillSync.Identity.Infrastructure.Outbox;

internal sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public DateTimeOffset OccuredAtUtc { get; private set; }
    public DateTimeOffset? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; } 

    private OutboxMessage() 
    {
    }

    public static OutboxMessage Create(string type, string content, DateTimeOffset occuredAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = content,
            OccuredAtUtc = occuredAtUtc.ToUniversalTime()
        };
    }

    public void MarkAsProcessed(DateTimeOffset processedOnUtc)
    {
        if (ProcessedOnUtc is not null)
            throw new InvalidOperationException("Outbox message has already been processed");

        ProcessedOnUtc = processedOnUtc.ToUniversalTime();
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        Error = error;
    }
}
