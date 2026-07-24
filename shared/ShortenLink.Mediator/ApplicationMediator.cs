using System.Collections.Concurrent;

namespace ShortenLink.Mediator;

public sealed class ApplicationMediator(
    MediatorServiceFactory serviceFactory,
    MediatorServiceEnumerableFactory enumerableFactory) : IMediator
{
    private static readonly ConcurrentDictionary<(Type Request, Type Response), RequestHandlerWrapper> RequestHandlers = new();
    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> NotificationHandlers = new();

    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = RequestHandlers.GetOrAdd(
            (request.GetType(), typeof(TResponse)),
            static key => (RequestHandlerWrapper)Activator.CreateInstance(
                typeof(RequestHandlerWrapper<,>).MakeGenericType(key.Request, key.Response))!);

        return wrapper.Handle(request, serviceFactory, enumerableFactory, cancellationToken);
    }

    public Task Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var wrapper = NotificationHandlers.GetOrAdd(
            notification.GetType(),
            static notificationType => (NotificationHandlerWrapper)Activator.CreateInstance(
                typeof(NotificationHandlerWrapper<>).MakeGenericType(notificationType))!);

        return wrapper.Handle(notification, enumerableFactory, cancellationToken);
    }

    private abstract class RequestHandlerWrapper
    {
        public abstract Task<TResponse> Handle<TResponse>(
            IRequest<TResponse> request,
            MediatorServiceFactory serviceFactory,
            MediatorServiceEnumerableFactory enumerableFactory,
            CancellationToken cancellationToken);
    }

    private sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper
        where TRequest : IRequest<TResponse>
    {
        public override async Task<TRequestedResponse> Handle<TRequestedResponse>(
            IRequest<TRequestedResponse> request,
            MediatorServiceFactory serviceFactory,
            MediatorServiceEnumerableFactory enumerableFactory,
            CancellationToken cancellationToken)
        {
            var typedRequest = (TRequest)(object)request;
            var handler = (IRequestHandler<TRequest, TResponse>)serviceFactory(
                typeof(IRequestHandler<TRequest, TResponse>));

            RequestHandlerDelegate<TResponse> next = () => handler.Handle(typedRequest, cancellationToken);
            var behaviors = enumerableFactory(typeof(IPipelineBehavior<TRequest, TResponse>))
                .Cast<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .ToArray();

            foreach (var behavior in behaviors)
            {
                var currentNext = next;
                next = () => behavior.Handle(typedRequest, currentNext, cancellationToken);
            }

            var response = await next().ConfigureAwait(false);
            return response is null
                ? default!
                : (TRequestedResponse)(object)response;
        }
    }

    private abstract class NotificationHandlerWrapper
    {
        public abstract Task Handle(
            INotification notification,
            MediatorServiceEnumerableFactory enumerableFactory,
            CancellationToken cancellationToken);
    }

    private sealed class NotificationHandlerWrapper<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        public override Task Handle(
            INotification notification,
            MediatorServiceEnumerableFactory enumerableFactory,
            CancellationToken cancellationToken)
        {
            var handlers = enumerableFactory(typeof(INotificationHandler<TNotification>))
                .Cast<INotificationHandler<TNotification>>();

            return Task.WhenAll(handlers.Select(handler =>
                handler.Handle((TNotification)notification, cancellationToken)));
        }
    }
}
