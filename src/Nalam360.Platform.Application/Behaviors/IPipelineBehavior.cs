namespace Nalam360.Platform.Application.Behaviors;

/// <summary>
/// Pipeline behavior that wraps request handling
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
/// Delegate representing the next action in the pipeline
/// </summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
