using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notegether.Api;
using Notegether.Api.BotClient;
using Notegether.Bll.Commands;
using Notegether.Bll.Services;

namespace Notegether;

public static class StartUp
{
    private static IConfiguration _configuration;

    static StartUp()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true);

        _configuration = builder.Build();
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<IConfiguration>(_configuration);
        services.AddScoped<ISayHelloService, SayHelloService>();
        services.AddScoped<NotegetherController>();
        services.AddScoped<BotHandlers>();

        services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(StartUp).Assembly));

        return services;
    }
}