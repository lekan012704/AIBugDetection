using ErrorOr;
using MediatR;

namespace Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<ErrorOr<TResponse>>, IBaseCommand;

public interface IBaseCommand;

