using MediatR;
using Notegether.Api.Requests;
using Notegether.Api.Responses;
using Notegether.Bll.Commands;
using Notegether.Bll.Models;
using Notegether.Bll.Models.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Notegether.Api;

public class NotegetherController
{
    private readonly IMediator _mediator;
    private Dictionary<ChatId, NoteFormStatus> _noteFormStatuses;
    private Dictionary<ChatId, NoteModel> _noteModels;

    public NotegetherController(IMediator mediator)
    {
        _mediator = mediator;
        _noteFormStatuses = new Dictionary<ChatId, NoteFormStatus>();
        _noteModels = new Dictionary<ChatId, NoteModel>();
    }


    public async Task SayHello(HelloRequest request)
    {
        var command = new SayHelloCommand(request.Message.Chat.Username);

        var greetingMessage = await _mediator.Send(command, request.CancellationToken);

        var botClient = request.BotClient;

        Message hiMessage = await botClient.SendTextMessageAsync(
            chatId: request.Message.Chat.Id,
            text: greetingMessage.Greeting,
            cancellationToken: request.CancellationToken);
    }


    public async Task<CreateNoteResponse> CreateNote(CreateNoteRequest request)
    {

        var chatId = request.Message.Chat.Id;
        var messageText = request.Message.Text;

        if (!_noteFormStatuses.ContainsKey(chatId))
        {
            _noteFormStatuses[chatId] = NoteFormStatus.Init;
            _noteModels[chatId] = new NoteModel();
        }

        switch (_noteFormStatuses[chatId])
        {
            case NoteFormStatus.Init:
                _noteFormStatuses[chatId] = NoteFormStatus.Name;
                break;
            
            case NoteFormStatus.Name:
                _noteModels[chatId].Name = messageText;
                await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"You input name = {_noteModels[chatId].Name}",
                    cancellationToken: request.CancellationToken);
                _noteFormStatuses[chatId] = NoteFormStatus.Description;
                break;

            case NoteFormStatus.Description:
                _noteModels[chatId].Description = messageText;
                await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"You input description = {_noteModels[chatId].Description}",
                    cancellationToken: request.CancellationToken);
                _noteFormStatuses[chatId] = NoteFormStatus.Text;
                break;
            
            case NoteFormStatus.Text:
                _noteModels[chatId].Text = messageText;
                await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"You input text = {_noteModels[chatId].Text}",
                    cancellationToken: request.CancellationToken);
                _noteFormStatuses[chatId] = NoteFormStatus.Ready;
                break;
            
        }
        

        if (_noteFormStatuses[chatId] == NoteFormStatus.Ready)
        {
            string textMessage = $"\nNote name = {_noteModels[chatId].Name}" +
                                 $"\nNote description = {_noteModels[chatId].Description}" +
                                 $"\nNote text: {_noteModels[chatId].Text}";

            await request.BotClient.SendTextMessageAsync(
                chatId: request.Message.Chat.Id,
                text: $"Form for creating note:" + textMessage,
                cancellationToken: request.CancellationToken);

            await _mediator.Send(new CreateNoteCommand("", "", ""));

            return new CreateNoteResponse(true);
        }

        return new CreateNoteResponse(false);
    }


}