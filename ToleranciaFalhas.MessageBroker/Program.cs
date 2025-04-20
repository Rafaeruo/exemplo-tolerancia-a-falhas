
namespace ToleranciaFalhas.MessageBroker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Map appsettings
            builder.Services.Configure<GatewayConfig>(builder.Configuration.GetSection("Gateway"));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<CircuitBreakerManager>(); // this calls the constructor with default parameter values; we can change that with a lambda on this very call

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
