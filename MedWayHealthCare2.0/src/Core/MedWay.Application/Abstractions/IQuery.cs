using MediatR;
using MedWay.Domain.Primitives;

namespace MedWay.Application.Abstractions;

/// <summary>
/// Marker interface for queries
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : Result
{
}
