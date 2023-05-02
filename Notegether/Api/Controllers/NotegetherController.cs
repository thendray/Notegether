using MediatR;
using Notegether.Api.Requests;
using Notegether.Bll.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Notegether.Api;

public class NotegetherController
{
    private readonly IMediator _mediator;

    public NotegetherController(IMediator mediator)
    {
        _mediator = mediator;
    }


    public async Task SayHello(HelloRequest request)
    {
        var command = new SayHelloCommand(request.Message.Chat.Username);
        
        var greetingMessage = await _mediator.Send(command);

        var botClient = request.BotClient;
        
        Message hiMessage = await botClient.SendTextMessageAsync(
            chatId: request.Message.Chat.Id,
            text: greetingMessage.Greeting,
            cancellationToken: request.CancellationToken);
    }
    
    
}