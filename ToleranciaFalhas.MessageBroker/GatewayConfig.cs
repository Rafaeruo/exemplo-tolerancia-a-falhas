namespace ToleranciaFalhas.MessageBroker;

public class GatewayConfig
{
    public required List<ServiceConfig> Services { get; set; }
}

public class ServiceConfig
{
    public required string Name { get; set; }
    public required string BaseUrl { get; set; }
    public required List<RouteConfig> Routes { get; set; }
}

public class RouteConfig
{
    public required string Path { get; set; }
    public required string HttpVerb { get; set; }
}