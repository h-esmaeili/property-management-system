using MediatR;

namespace PMS.Application.CQRS.Abstractions;

public interface IQuery<out T> : IRequest<T>;
