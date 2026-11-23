using Microsoft.Extensions.Logging;

namespace SkillSync.Identity.Infrastructure.Outbox;

internal static partial class OutboxProcessorLogs
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Failed to process outbox message {MessageId}.")]
    public static partial void OutboxMessageFailed(
        ILogger logger,
        Exception exception,
        Guid messageId);
}