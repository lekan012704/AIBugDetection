using ErrorOr;
using MediatR;

namespace Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<ErrorOr<TResponse>>;
