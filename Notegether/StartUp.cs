using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}