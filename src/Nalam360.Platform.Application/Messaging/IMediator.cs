namespace Nalam360.Platform.Application.Messaging;

/// <summary>
/// Sends requests to their handlers
/// </summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
