using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Application.Messaging;

/// <summary>
/// Marker interface for commands
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Marker interface for commands with result
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
