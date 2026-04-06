using MediatR;
using ErrorOr;

namespace Application.Abstractions.Messaging
{
    public interface IRequestWrapper<TResponse> : IRequest<ErrorOr<TResponse>>
    {
    }

    public interface IHandlerWrapper<in TRequest, TResponse> : IRequestHandler<TRequest, ErrorOr<TResponse>>
        where TRequest : IRequestWrapper<TResponse>
    {
    }
}
