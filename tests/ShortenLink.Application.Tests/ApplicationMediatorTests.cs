using ShortenLink.Mediator;
using Xunit;

namespace ShortenLink.Application.Tests;

public sealed class ApplicationMediatorTests
{
    [Fact]
    public async Task Send_DispatchesRequestThroughPipelineInRegistrationOrder()
    {
        var trace = new List<string>();
        var handler = new PingHandler(trace);
        var behaviors = new object[]
        {
            new TraceBehavior("outer", trace),
            new TraceBehavior("inner", trace)
        };
        var mediator = CreateMediator(
            new Dictionary<Type, object>
            {
                [typeof(IRequestHandler<Ping, string>)] = handler
            },
            new Dictionary<Type, IReadOnlyList<object>>
            {
                [typeof(IPipelineBehavior<Ping, string>)] = behaviors
            });

        var response = await mediator.Send(new Ping("hello"));

        Assert.Equal("HELLO", response);
        Assert.Equal(
            ["outer:before", "inner:before", "handler", "inner:after", "outer:after"],
            trace);
    }

    [Fact]
    public async Task Publish_NotifiesEveryRegisteredHandler()
    {
        var trace = new List<string>();
        var mediator = CreateMediator(
            new Dictionary<Type, object>(),
            new Dictionary<Type, IReadOnlyList<object>>
            {
                [typeof(INotificationHandler<Pinged>)] =
                [
                    new PingedHandler("one", trace),
                    new PingedHandler("two", trace)
                ]
            });

        await mediator.Publish(new Pinged());

        Assert.Equal(["one", "two"], trace.OrderBy(static value => value));
    }

    private static ApplicationMediator CreateMediator(
        IReadOnlyDictionary<Type, object> services,
        IReadOnlyDictionary<Type, IReadOnlyList<object>> collections) =>
        new(
            type => services.TryGetValue(type, out var service)
                ? service
                : throw new InvalidOperationException($"Missing service {type}."),
            type => collections.TryGetValue(type, out var values)
                ? values
                : []);

    private sealed record Ping(string Value) : IRequest<string>;

    private sealed class PingHandler(List<string> trace) : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        {
            trace.Add("handler");
            return Task.FromResult(request.Value.ToUpperInvariant());
        }
    }

    private sealed class TraceBehavior(string name, List<string> trace)
        : IPipelineBehavior<Ping, string>
    {
        public async Task<string> Handle(
            Ping request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            trace.Add($"{name}:before");
            var response = await next();
            trace.Add($"{name}:after");
            return response;
        }
    }

    private sealed record Pinged : INotification;

    private sealed class PingedHandler(string name, List<string> trace)
        : INotificationHandler<Pinged>
    {
        public Task Handle(Pinged notification, CancellationToken cancellationToken)
        {
            trace.Add(name);
            return Task.CompletedTask;
        }
    }
}
