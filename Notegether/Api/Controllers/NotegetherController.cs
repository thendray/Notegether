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
using Telegram.Bot.Types.ReplyMarkups;

namespace Notegether.Api.Controllers;

public class NotegetherController
{
    private readonly IMediator _mediator;

    private readonly Dictionary<ChatId, ProcessStatus> _commandProcessStatuses;
    private readonly Dictionary<ChatId, SaveModel> _savedData;
    private readonly Dictionary<ChatId, List<int>> _chatMessages;
    public NotegetherController(IMediator mediator)
    {
        _mediator = mediator;
        _chatMessages = new Dictionary<ChatId, List<int>>();
        _savedData = new Dictionary<ChatId, SaveModel>();

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
        var chatId = request.Message.Chat.Id;
        var messageText = request.Message.Text;

        if (!_commandProcessStatuses.ContainsKey(chatId) ||
            _commandProcessStatuses[chatId]  == ProcessStatus.None ||
            request.Message.Text == "/create_note")
        {
            _commandProcessStatuses[chatId] = ProcessStatus.Init;
            _savedData[chatId] = new SaveModel();
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
                _savedData[chatId].Value1 = messageText;
                var nameMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"Добавлено название: {_savedData[chatId].Value1}\n<b>Надо добавить короткое описание заметки:</b>",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                _commandProcessStatuses[chatId] = ProcessStatus.SecondStep;
                
                _chatMessages[chatId].Add(nameMessage.MessageId);
                _chatMessages[chatId].Add(request.Message.MessageId);
                break;

            case ProcessStatus.SecondStep:
                _savedData[chatId].Value2 = messageText;
                var descriptionMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"Добавлено описание:{_savedData[chatId].Value2}\n<b>Следующее сообщение - ваша заметка</b>",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                _commandProcessStatuses[chatId] = ProcessStatus.ThirdStep;
                
                _chatMessages[chatId].Add(descriptionMessage.MessageId);
                _chatMessages[chatId].Add(request.Message.MessageId);
                break;
            
            case ProcessStatus.ThirdStep:
                _savedData[chatId].Value3 = messageText;
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
            var name = _savedData[chatId].Value1;
            var description = _savedData[chatId].Value2;
            var text = _savedData[chatId].Value3;
            

            Message loadingMessage = await request.BotClient.SendTextMessageAsync(
                chatId: request.Message.Chat.Id,
                text: "<i>Создаем заметку ....</i>",
                cancellationToken: request.CancellationToken,
                parseMode: ParseMode.Html);

            var result = await _mediator.Send(new CreateNoteCommand(chatId, name, description, text));

            await request.BotClient.EditMessageTextAsync(chatId, loadingMessage.MessageId, result.Answer, parseMode: ParseMode.Html);
            
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
            _commandProcessStatuses[chatId] == ProcessStatus.None ||
            request.Message.Text == "/delete_note")
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
                
                var result = await _mediator.Send(new DeleteNoteCommand(request.Message.Text, request.Message.From.Id));
                
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
            _commandProcessStatuses[chatId] == ProcessStatus.None ||
            request.Message.Text == "/edit_note")
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
                _savedData[chatId] = new SaveModel
                {
                    Value1 = request.Message.Text
                };
                break;
            
            case ProcessStatus.SecondStep:
                _savedData[chatId].Value2 = queryAnswer;
                _commandProcessStatuses[chatId] = ProcessStatus.ThirdStep;

                await request.BotClient.EditMessageReplyMarkupAsync(
                    chatId,
                    _chatMessages[chatId][_chatMessages[chatId].Count - 1],
                    replyMarkup: MenuButtons.ObjectForEditingMockInlineKeyboardMarkup());
                
                var editTypeMessge = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"Выберите как, следует изменить:",
                    cancellationToken: request.CancellationToken,
                    replyMarkup: MenuButtons.EditTypeInlineKeyboardMarkup(),
                    parseMode: ParseMode.Html);
               
                _chatMessages[chatId].Add(editTypeMessge.MessageId);
                break;
            
            case ProcessStatus.ThirdStep:
                _savedData[chatId].Value3 = queryAnswer;

                await request.BotClient.EditMessageReplyMarkupAsync(
                    chatId,
                    _chatMessages[chatId][_chatMessages[chatId].Count - 1],
                    replyMarkup: MenuButtons.EditTypeInlineMockKeyboardMarkup());
                
                var queryMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"<b>Отлично!</b>\nСделайте необходимые изменения:",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);

                _commandProcessStatuses[chatId] = ProcessStatus.FourthStep;
                _chatMessages[chatId].Add(queryMessage.MessageId);
                break;
                
            case ProcessStatus.FourthStep:
                _chatMessages[chatId].Add(request.Message.MessageId);
                
                var command = new EditNoteCommand(
                    _savedData[chatId].Value1,
                    _savedData[chatId].Value2,
                    request.Message.Text,
                    request.Message.Chat.Id,
                    request.Message.From.Username,
                    _savedData[chatId].Value3);

                var result = await _mediator.Send(command, request.CancellationToken);
                
                await request.BotClient.SendTextMessageAsync
                    (chatId, result.ReadyAnswer, parseMode: ParseMode.Html);

                if (result.OthersChatId.Count > 0)
                {
                    foreach (var id in result.OthersChatId)
                    {
                        await request.BotClient.SendTextMessageAsync
                            (id, result.MessageForOthers, parseMode: ParseMode.Html);
                    }
                }

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

        var command = new GetNoteCommand(GettingType.GetMyNotes, request.Message.From.Id, null);

        var result = await _mediator.Send(command, request.CancellationToken);
        
        await request.BotClient.SendTextMessageAsync
            (chatId, result, parseMode: ParseMode.Html);
    }
    public async Task<BasicResponse> GetMyNote(BasicRequest request)
    {
        var chatId = request.Message.Chat.Id;

        if (!_commandProcessStatuses.ContainsKey(chatId) ||
            _commandProcessStatuses[chatId] == ProcessStatus.None || 
            request.Message.Text == "/get_my_note")
        {
            _commandProcessStatuses[chatId] = ProcessStatus.Init;
        }

        switch (_commandProcessStatuses[chatId])
        {
            case ProcessStatus.Init:
                var startMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: "<b>Введите идентификатор заметки:</b>",
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
                    text: $"<b>Идентефикатор получен</b>\nИщем заметку....",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);
                
                var result = await _mediator.Send(new GetNoteCommand(
                    GettingType.GetOneNote, request.Message.From.Id, request.Message.Text));
                
                await request.BotClient.EditMessageTextAsync(chatId,
                    getMessage.MessageId,
                    result,
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
    public async Task<BasicResponse> AddPermission(BasicRequest request, string queryAnswer = "")
    {
        var chatId = request.Message.Chat.Id;
        
        if (!_commandProcessStatuses.ContainsKey(chatId) ||
            _commandProcessStatuses[chatId] == ProcessStatus.None ||
            request.Message.Text == "/add_permission")
        {
            _commandProcessStatuses[chatId] = ProcessStatus.Init;
        }

        switch (_commandProcessStatuses[chatId])
        {
            case ProcessStatus.Init:
                var startingEditingMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"<b>Готов к добавлению разрешения!</b>\nНадо ввести идентификатор заметки, к которой будет предоставлен доступ: ",
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
                    text: $"<b>Идентефикатор получен</b>\nВведите имя пользователя, которому хотите предоставить доступ (без @)",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);

                _commandProcessStatuses[chatId] = ProcessStatus.SecondStep;
                _chatMessages[chatId].Add(getMessage.MessageId);
                _savedData[chatId] = new SaveModel
                {
                    Value1 = request.Message.Text
                };
                break;
            
            
            case ProcessStatus.SecondStep:
                _savedData[chatId].Value2 = request.Message.Text;
                _savedData[chatId].Value3 = request.Message.From.Username;

                var queryMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"<b>Отлично!</b>\nВыберите уровень доступа",
                    replyMarkup: MenuButtons.PermissionsRolesInlineKeyboardMarkup(),
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);

                _commandProcessStatuses[chatId] = ProcessStatus.ThirdStep;
                _chatMessages[chatId].Add(queryMessage.MessageId);
                _chatMessages[chatId].Add(request.Message.MessageId);
                break;
                
            case ProcessStatus.ThirdStep:

                var command = new AddPermissionCommand(
                    _savedData[chatId].Value1,
                    _savedData[chatId].Value2,
                    _savedData[chatId].Value3,
                    chatId,
                    queryAnswer);

                var result = await _mediator.Send(command, request.CancellationToken);
                
                await request.BotClient.SendTextMessageAsync
                    (chatId, result.WhoGiveMessage, parseMode: ParseMode.Html);

                if (result.IsSuccess)
                {
                    await request.BotClient.SendTextMessageAsync(result.WhoGetChatId,
                        result.WhoGetMessage,
                        parseMode: ParseMode.Html);
                }
                
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
    
    public async Task GetOtherNotes(BasicRequest request)
    {
        var chatId = request.Message.Chat.Id;

        var command = new GetNoteCommand(GettingType.GetOtherNotes, request.Message.From.Id, null);

        var result = await _mediator.Send(command, request.CancellationToken);
        
        await request.BotClient.SendTextMessageAsync
            (chatId, result, parseMode: ParseMode.Html);
    }
    public async Task<BasicResponse> DeletePermission(BasicRequest request)
    {
        
          var chatId = request.Message.Chat.Id;
        
        if (!_commandProcessStatuses.ContainsKey(chatId) ||
            _commandProcessStatuses[chatId] == ProcessStatus.None ||
            request.Message.Text == "/delete_permission")
        {
            _commandProcessStatuses[chatId] = ProcessStatus.Init;
        }

        switch (_commandProcessStatuses[chatId])
        {
            case ProcessStatus.Init:
                var startingEditingMessage = await request.BotClient.SendTextMessageAsync(
                    chatId: request.Message.Chat.Id,
                    text: $"<b>Готов к удалению разрешения!</b>\nНадо ввести идентификатор заметки, к которой следует удалить доступ: ",
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
                    text: $"<b>Идентефикатор получен</b>\nВведите имя пользователя, у которого хотите забрать доступ (без @)",
                    cancellationToken: request.CancellationToken,
                    parseMode: ParseMode.Html);

                _commandProcessStatuses[chatId] = ProcessStatus.SecondStep;
                _chatMessages[chatId].Add(getMessage.MessageId);
                _savedData[chatId] = new SaveModel
                {
                    Value1 = request.Message.Text
                };
                break;
            
                
            case ProcessStatus.SecondStep:
                _chatMessages[chatId].Add(request.Message.MessageId);

                _savedData[chatId].Value2 = request.Message.Text;

                var command = new DeletePermissionCommand(
                    _savedData[chatId].Value1,
                    _savedData[chatId].Value2,
                    chatId);

                var result = await _mediator.Send(command, request.CancellationToken);
                
                await request.BotClient.SendTextMessageAsync
                    (chatId, result, parseMode: ParseMode.Html);

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
    public async Task GetGivePermissions(BasicRequest request)
    {
        var chatId = request.Message.Chat.Id;

        var command = new GetPermissionsCommand(chatId);

        var result = await _mediator.Send(command, request.CancellationToken);
        
        await request.BotClient.SendTextMessageAsync
            (chatId, result, parseMode: ParseMode.Html);
    }
}