using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notegether;
using Telegram.Bot;

IServiceCollection serviceCollection = new ServiceCollection();

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true);

IConfiguration configuration = builder.Build();

serviceCollection.AddSingleton<IConfiguration>(configuration);
serviceCollection.AddScoped<Test>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var test = serviceProvider.GetService<Test>();
test.Print();


var appSettings = serviceProvider.GetService<IConfiguration>().GetSection("Settings").Get<Settings>();
Console.WriteLine(appSettings.Token);



var token = "5685813896:AAHVP5HykQDHqFGpfz5xdlNCcQg8Fnib9K0";

var botClient = new TelegramBotClient(token);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");