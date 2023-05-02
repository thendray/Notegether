using Notegether.Api.Requests;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Notegether.Api.BotClient;

public class BotHandlers
{
    private readonly NotegetherController _controller;

    public BotHandlers(NotegetherController controller)
    {
        _controller = controller;
    }
    
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
        {
            return;
        }

        // Only process text messages
        // if (message.Text is not { } messageText)
        // {
        //     return;
        // }
        
        
        switch (message.Type)
        {
            case MessageType.Text:
                await TextMessageHandle(message, botClient, cancellationToken);
                break;
        }
        
        
        
    }

    public Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }


    private async Task TextMessageHandle(
        Message message,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        var messageText = message.Text;
        var chatId = message.Chat.Id;

        switch (messageText)
        {
            case "/hello":
                await _controller.SayHello(new HelloRequest(botClient, message, cancellationToken));
                break;
        }

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
        
        // Echo received message text
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: cancellationToken);
    }
}