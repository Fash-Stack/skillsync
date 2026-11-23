using SkillSync.SharedKernel.Messaging;

namespace SkillSync.Worker.Outbox;

internal sealed class OutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan ProcessingInterval =
        TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var processor = scope.ServiceProvider
                    .GetRequiredService<IOutboxProcessor>();

                await processor.ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.Log(
                    LogLevel.Error,
                    new EventId(1, nameof(OutboxBackgroundService)),
                    "Outbox processing failed.",
                    exception,
                    static (state, _) => state);
            }

            try
            {
                await Task.Delay(ProcessingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}