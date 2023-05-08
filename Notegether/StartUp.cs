using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notegether.Api;
using Notegether.Api.BotClient;
using Notegether.Api.Controllers;
using Notegether.Bll.Models;
using Notegether.Bll.Services;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;
using Notegether.Dal.Entities;
using Notegether.Dal.Repositories;
using Telegram.Bot.Types;

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
        
        services.Configure<Settings>(_configuration.GetSection("Settings"));
        
        services.AddScoped<ISayHelloService, SayHelloService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<NotegetherController>();
        services.AddScoped<BotHandlers>();
        
        services.AddDbContext<MyDbContext>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        
        services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(StartUp).Assembly));

        return services;
    }
}