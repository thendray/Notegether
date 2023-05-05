using MediatR;
using Notegether.Api.Requests;
using Notegether.Api.Responses;
using Notegether.Bll.Commands;
using Notegether.Bll.Models;
using Notegether.Bll.Models.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Notegether.Api.Controllers;

public class NotegetherController
{
    private readonly IMediator _mediator;
    private readonly Dictionary<ChatId, NoteFormStatus> _noteFormStatuses;
    private readonly Dictionary<ChatId, NoteModel> _noteModels;
    private readonly Dictionary<ChatId, List<int>> _chatMessages;
    private readonly Dictionary<ChatId, NoteDeleteStatus> _deleteStatuses;
    public NotegetherController(IMediator mediator)
    {
        _mediator = mediator;
        _noteFormStatuses = new Dictionary<ChatId, NoteFormStatus>();
        _noteModels = new Dictionary<ChatId, NoteModel>();
        _chatMessages = new Dictionary<ChatId, List<int>>();
        _deleteStatuses = new Dictionary<ChatId, NoteDeleteStatus>();
    }
    

    public async Task SayHello(BasicRequest request)
    {
        var command = new SayHelloCommand(request.Message.Chat.Username);

        var greetingMessage = await _mediator.Send(command, request.CancellationToken);

        var botClient = request.BotClient;

        Message hiMessage = await botClient.SendTextMessageAsync(
            chatId: request.Message.Chat.Id,
            text: greetingMessage.Greeting,
            cancellationToken: request.CancellationToken);
    }

    public async Task Start(BasicRequest request)
    {
        var command = new StartCommand(request.Message.From.Username, request.Message.Chat.Id,  request.Message.From.Id);
        await _mediator.Send(command, request.CancellationToken);
        await SayHello(request);
    }


    public async Task<BasicResponse> CreateNote(CreateNoteRequest request)
    {

        var chatId = request.Message.Chat.Id;
        var messageText = request.Message.Text;

        if (!_noteFormStatuses.ContainsKey(chatId) || _noteFormStatuses[chatId]  == NoteFormStatus.None)
        {
            _noteFormStatuses[chatId] = NoteFormStatus.Init;
            _noteModels[chatId] = new NoteModel();
        }

        switch (_noteFormStatuses[chatId])
        {
            case NoteFormStatus.Init:
                var startMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: "<b>Супер, готов создать новую заметку!</b>\nНадо ввести название заметки:",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);

                _chatMessages[chatId] = new List<int>();

                if (!_chatMessages.ContainsKey(chatId))
                {
                    _chatMessages[chatId] = new List<int>();
                }
                _chatMessages[chatId].Add(startMessage.MessageId);
                _noteFormStatuses[chatId] = NoteFormStatus.Name;
                break;
            
            case NoteFormStatus.Name:
                _noteModels[chatId].Name = messageText;
                var nameMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"Добавлено название: {_noteModels[chatId].Name}\n<b>Надо добавить короткое описание заметки:</b>",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                _noteFormStatuses[chatId] = NoteFormStatus.Description;
                
                _chatMessages[chatId].Add(nameMessage.MessageId);
                _chatMessages[chatId].Add(request.Message.MessageId);
                break;

            case NoteFormStatus.Description:
                _noteModels[chatId].Description = messageText;
                var descriptionMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"Добавлено описание:{_noteModels[chatId].Description}\n<b>Следующее сообщение - ваша заметка</b>",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                _noteFormStatuses[chatId] = NoteFormStatus.Text;
                
                _chatMessages[chatId].Add(descriptionMessage.MessageId);
                _chatMessages[chatId].Add(request.Message.MessageId);
                break;
            
            case NoteFormStatus.Text:
                _noteModels[chatId].Text = messageText;
                var readyMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"Текст заметки получен!",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                _noteFormStatuses[chatId] = NoteFormStatus.Ready;
                
                _chatMessages[chatId].Add(readyMessage.MessageId);
                _chatMessages[chatId].Add(request.Message.MessageId);
                break;
            
        }
        

        if (_noteFormStatuses[chatId] == NoteFormStatus.Ready)
        {
            var name = _noteModels[chatId].Name;
            var description = _noteModels[chatId].Description;
            var text = _noteModels[chatId].Text;
            

            Message loadingMessage = await request.BotClient.SendTextMessageAsync(
                chatId: request.Message.Chat.Id,
                text: "<i>Создаем заметку ....</i>",
                cancellationToken: request.CancellationToken,
                parseMode: ParseMode.Html);

            var result = await _mediator.Send(new CreateNoteCommand(chatId, name, description, text));

            string textMessage = $"<b>Заметка создана :)</b>\n" +
                                 $"Названеи = {name}\n" +
                                 $"Описание = {description}\n" +
                                 $"Краткий идентификатор = {result.Identifier}";
            
            await request.BotClient.EditMessageTextAsync(chatId, loadingMessage.MessageId, textMessage, parseMode: ParseMode.Html);
            
            foreach (var message in _chatMessages[chatId])
            {
                await request.BotClient.DeleteMessageAsync(chatId, message, request.CancellationToken);
            }
            
            _chatMessages[chatId].Clear();
            _noteFormStatuses[chatId] = NoteFormStatus.None;
            return new BasicResponse(true);
        }

        return new BasicResponse(false);
    }


    public async Task<BasicResponse> DeleteNote(BasicRequest request, string identifier = "")
    {
        var chatId = request.Message.Chat.Id;

        if (!_deleteStatuses.ContainsKey(chatId) || _deleteStatuses[chatId] == NoteDeleteStatus.None)
        {
            _deleteStatuses[chatId] = NoteDeleteStatus.Init;
        }

        switch (_deleteStatuses[chatId])
        {
            case NoteDeleteStatus.Init:
                var startMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: "<b>Готов удалять заметку!</b>\nНадо ввести идентификатор заметки:",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                
                if (!_chatMessages.ContainsKey(chatId))
                {
                    _chatMessages[chatId] = new List<int>();
                }
                _deleteStatuses[chatId] = NoteDeleteStatus.GetIdentifier;
                _chatMessages[chatId].Add(startMessage.MessageId);
                break;
            
            case NoteDeleteStatus.GetIdentifier:
                
                _chatMessages[chatId].Add(request.Message.MessageId);

                var getMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"<b>Идентефикатор получен</b>\nУдаляем заметку....",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                
                var result = await _mediator.Send(new DeleteNoteCommand(request.Message.Text));
                
                await request.BotClient.EditMessageTextAsync(chatId,
                    getMessage.MessageId,
                    result.OutputMessage,
                    parseMode: ParseMode.Html);

                _deleteStatuses[chatId] = NoteDeleteStatus.Ready;
                break;
        }

        if (_deleteStatuses[chatId] == NoteDeleteStatus.Ready)
        {
            foreach (var message in _chatMessages[chatId])
            {
                await request.BotClient.DeleteMessageAsync(chatId, message);
            }
            _chatMessages[chatId].Clear();
            _deleteStatuses[chatId] = NoteDeleteStatus.None;
            return new BasicResponse(true);
        }

        return new BasicResponse(false);

    }
        
    
}