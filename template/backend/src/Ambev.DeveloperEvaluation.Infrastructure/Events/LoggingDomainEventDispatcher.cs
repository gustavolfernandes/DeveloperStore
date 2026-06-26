using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Infrastructure.Events;

/// <summary>
/// <see cref="IDomainEventDispatcher"/> implementation that records each dispatched
/// domain event in the application log.
/// </summary>
public class LoggingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<LoggingDomainEventDispatcher> _logger;

    public LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation(
                "Domain event {EventType} occurred at {OccurredOn:o}: {@Event}",
                domainEvent.GetType().Name,
                domainEvent.OccurredOn,
                domainEvent);
        }

        return Task.CompletedTask;
    }
}
