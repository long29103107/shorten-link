namespace ShortenLink.Mediator;

public delegate object MediatorServiceFactory(Type serviceType);

public delegate IEnumerable<object> MediatorServiceEnumerableFactory(Type serviceType);
