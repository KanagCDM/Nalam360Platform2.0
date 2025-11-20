using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Application.Messaging;

/// <summary>
/// Marker interface for queries
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
