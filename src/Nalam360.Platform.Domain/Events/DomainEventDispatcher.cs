using Microsoft.Extensions.DependencyInjection;

namespace Nalam360.Platform.Domain.Events;

/// <summary>
/// Default implementation of domain event dispatcher using service provider
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<TEvent>.Handle));
            if (handleMethod != null)
            {
                var task = (Task?)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken });
                if (task != null)
                {
                    await task;
                }
            }
        }
    }

    public async Task DispatchAsync(IEnumerable<object> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            var dispatchMethod = GetType()
                .GetMethod(nameof(DispatchAsync), new[] { domainEvent.GetType(), typeof(CancellationToken) });

            if (dispatchMethod != null)
            {
                var task = (Task?)dispatchMethod.Invoke(this, new[] { domainEvent, cancellationToken });
                if (task != null)
                {
                    await task;
                }
            }
        }
    }
}
