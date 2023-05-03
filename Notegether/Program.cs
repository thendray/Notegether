using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notegether;
using Notegether.Api.BotClient;
using Telegram.Bot;

IServiceCollection serviceCollection = new ServiceCollection();
serviceCollection.ConfigureServices();

var serviceProvider = serviceCollection.BuildServiceProvider();
// var appSettings = serviceProvider.GetService<IConfiguration>()?.GetSection("Settings").Get<Settings>();

var appSettings = serviceProvider.GetRequiredService<IOptions<Settings>>();
using CancellationTokenSource cts = new();

var token = appSettings.Value.Token;

var botClient = new TelegramBotClient(token);


var handler = serviceProvider.GetService<BotHandlers>();

botClient.ConfigureBot(
    cts.Token,
    handler.HandleUpdateAsync,
    handler.HandlePollingErrorAsync);



var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();
