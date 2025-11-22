using MediatR;
using MedWay.Domain.Primitives;

namespace MedWay.Application.Abstractions;

/// <summary>
/// Marker interface for commands that return a result
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
    where TResponse : Result
{
}

/// <summary>
/// Marker interface for commands without return value
/// </summary>
public interface ICommand : IRequest<Result>
{
}
