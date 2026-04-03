using MediatR;

namespace PMS.Application.CQRS.Abstractions;

public interface ICommand : IRequest;

public interface ICommand<out T> : IRequest<T>;
