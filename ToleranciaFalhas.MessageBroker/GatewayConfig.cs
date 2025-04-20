namespace ToleranciaFalhas.MessageBroker;

public class GatewayConfig
{
    public List<ServiceConfig> Services { get; set; }
}

public class ServiceConfig
{
    public string Name { get; set; }
    public string BaseUrl { get; set; }
    public List<RouteConfig> Routes { get; set; }
}

public class RouteConfig
{
    public string Path { get; set; }
    public string HttpVerb { get; set; }
}