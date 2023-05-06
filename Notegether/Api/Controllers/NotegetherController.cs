using MediatR;
using Notegether.Api.BotClient.Markups;
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

    private readonly Dictionary<ChatId, ProcessStatus> _commandProcessStatuses;
    private readonly Dictionary<ChatId, NoteModel> _noteModels;
    private readonly Dictionary<ChatId, Tuple<string, string>> _editModels;
    private readonly Dictionary<ChatId, List<int>> _chatMessages;
    public NotegetherController(IMediator mediator)
    {
        _mediator = mediator;
        _noteModels = new Dictionary<ChatId, NoteModel>();
        _chatMessages = new Dictionary<ChatId, List<int>>();
        _editModels = new Dictionary<ChatId, Tuple<string, string>>();

        _commandProcessStatuses = new Dictionary<ChatId, ProcessStatus>();
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

        var command = CommandStatus.CreateNote;
        var chatId = request.Message.Chat.Id;
        var messageText = request.Message.Text;

        if (!_commandProcessStatuses.ContainsKey(chatId) ||
            _commandProcessStatuses[chatId]  == ProcessStatus.None)
        {
            _commandProcessStatuses[chatId] = ProcessStatus.Init;
            _noteModels[chatId] = new NoteModel();
        }

        switch (_commandProcessStatuses[chatId])
        {
            case ProcessStatus.Init:
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
                _commandProcessStatuses[chatId] = ProcessStatus.FirstStep;
                break;
            
            case ProcessStatus.FirstStep:
                _noteModels[chatId].Name = messageText;
                var nameMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"Добавлено название: {_noteModels[chatId].Name}\n<b>Надо добавить короткое описание заметки:</b>",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                _commandProcessStatuses[chatId] = ProcessStatus.SecondStep;
                
                _chatMessages[chatId].Add(nameMessage.MessageId);
                _chatMessages[chatId].Add(request.Message.MessageId);
                break;

            case ProcessStatus.SecondStep:
                _noteModels[chatId].Description = messageText;
                var descriptionMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"Добавлено описание:{_noteModels[chatId].Description}\n<b>Следующее сообщение - ваша заметка</b>",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                _commandProcessStatuses[chatId] = ProcessStatus.ThirdStep;
                
                _chatMessages[chatId].Add(descriptionMessage.MessageId);
                _chatMessages[chatId].Add(request.Message.MessageId);
                break;
            
            case ProcessStatus.ThirdStep:
                _noteModels[chatId].Text = messageText;
                var readyMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"<b>Текст заметки получен!</b>",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                _commandProcessStatuses[chatId] = ProcessStatus.Ready;
                
                _chatMessages[chatId].Add(readyMessage.MessageId);
                _chatMessages[chatId].Add(request.Message.MessageId);
                break;
            
        }
        

        if (_commandProcessStatuses[chatId] == ProcessStatus.Ready)
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
            _commandProcessStatuses[chatId] = ProcessStatus.None;
            return new BasicResponse(true);
        }

        return new BasicResponse(false);
    }


    public async Task<BasicResponse> DeleteNote(BasicRequest request)
    {
        var chatId = request.Message.Chat.Id;
        var command = CommandStatus.DeleteNote;

        if (!_commandProcessStatuses.ContainsKey(chatId) ||
            _commandProcessStatuses[chatId] == ProcessStatus.None)
        {
            _commandProcessStatuses[chatId] = ProcessStatus.Init;
        }

        switch (_commandProcessStatuses[chatId])
        {
            case ProcessStatus.Init:
                var startMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: "<b>Готов удалять заметку!</b>\nНадо ввести идентификатор заметки:",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                
                if (!_chatMessages.ContainsKey(chatId))
                {
                    _chatMessages[chatId] = new List<int>();
                }
                _commandProcessStatuses[chatId] = ProcessStatus.FirstStep;
                _chatMessages[chatId].Add(startMessage.MessageId);
                break;
            
            case ProcessStatus.FirstStep:
                
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

                _commandProcessStatuses[chatId] = ProcessStatus.Ready;
                break;
        }

        if (_commandProcessStatuses[chatId] == ProcessStatus.Ready)
        {
            foreach (var message in _chatMessages[chatId])
            {
                await request.BotClient.DeleteMessageAsync(chatId, message);
            }
            _chatMessages[chatId].Clear();
            _commandProcessStatuses[chatId] = ProcessStatus.None;
            return new BasicResponse(true);
        }

        return new BasicResponse(false);

    }


    public async Task<BasicResponse> EditNote(BasicRequest request, string queryAnswer = "")
    {
        
        var chatId = request.Message.Chat.Id;

        if (!_commandProcessStatuses.ContainsKey(chatId) ||
            _commandProcessStatuses[chatId] == ProcessStatus.None)
        {
            _commandProcessStatuses[chatId] = ProcessStatus.Init;
        }

        switch (_commandProcessStatuses[chatId])
        {
            case ProcessStatus.Init:
                var startingEditingMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"<b>Готов к внесению изменений!</b>\nНадо ввести идентификатор заметки: ",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                
                if (!_chatMessages.ContainsKey(chatId))
                {
                    _chatMessages[chatId] = new List<int>();
                }
                _commandProcessStatuses[chatId] = ProcessStatus.FirstStep;
                _chatMessages[chatId].Add(startingEditingMessage.MessageId);
                break;
            
            case ProcessStatus.FirstStep:
                _chatMessages[chatId].Add(request.Message.MessageId);

                var getMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"<b>Идентефикатор получен</b>\nВыберите то, что следует изменить",
                    cancellationToken: request.CancellationToken,
                    replyMarkup: MenuButtons.ObjectForEditingInlineKeyboardMarkup(),
                    parseMode: ParseMode.Html);

                _commandProcessStatuses[chatId] = ProcessStatus.SecondStep;
                _chatMessages[chatId].Add(getMessage.MessageId);
                _editModels[chatId] = new Tuple<string, string>(request.Message.Text, "");
                break;
            
            
            case ProcessStatus.SecondStep:
                _editModels[chatId] = new Tuple<string, string>(_editModels[chatId].Item1, queryAnswer);

                var queryMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"<b>Отлично!</b>\nВведите обновленные данные:",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);

                _commandProcessStatuses[chatId] = ProcessStatus.ThirdStep;
                _chatMessages[chatId].Add(queryMessage.MessageId);
                break;
                
            case ProcessStatus.ThirdStep:
                _chatMessages[chatId].Add(request.Message.MessageId);
                
                var command = new EditNoteCommand(
                    _editModels[chatId].Item1,
                    _editModels[chatId].Item2,
                    request.Message.Text);

                var result = await _mediator.Send(command, request.CancellationToken);
                
                await request.BotClient.SendTextMessageAsync
                    (chatId, result.ReadyAnswer, parseMode: ParseMode.Html);

                _commandProcessStatuses[chatId] = ProcessStatus.Ready;
                
                break;
        }
        
        
        if (_commandProcessStatuses[chatId] == ProcessStatus.Ready)
        {
            foreach (var message in _chatMessages[chatId])
            {
                await request.BotClient.DeleteMessageAsync(chatId, message);
            }
            _chatMessages[chatId].Clear();
            _commandProcessStatuses[chatId] = ProcessStatus.None;
            return new BasicResponse(true);
        }

        return new BasicResponse(false);
        
    }
    public async Task GetMyNotes(BasicRequest request)
    {
        var chatId = request.Message.Chat.Id;

        // var command = new GetNotesCommand();

        // var result = await _mediator.Send(command, request.CancellationToken);
        
        

    }
}