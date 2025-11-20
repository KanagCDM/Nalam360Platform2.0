using Microsoft.Extensions.DependencyInjection;

namespace Nalam360.Platform.Application.Messaging;

/// <summary>
/// Default mediator implementation
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetRequiredService(handlerType);

        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));
        
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handler for request type {requestType.Name} does not have a Handle method");
        }

        var task = (Task<TResponse>?)handleMethod.Invoke(handler, new object[] { request, cancellationToken });

        if (task == null)
        {
            throw new InvalidOperationException($"Handler for request type {requestType.Name} returned null");
        }

        return await task;
    }
}
