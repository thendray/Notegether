﻿using Notegether.Api.Controllers;
using Notegether.Api.Requests;
using Notegether.Bll.Models;
using Notegether.Bll.Models.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Notegether.Api.BotClient;

public class BotHandlers
{
    private readonly NotegetherController _controller;

    private readonly Dictionary<ChatId, CommandStatus> _commandStatuses;
    private readonly Dictionary<ChatId, List<string>> _isClicked;

    public BotHandlers(NotegetherController controller)
    {
        _controller = controller;
        _commandStatuses = new Dictionary<ChatId, CommandStatus>();
        _isClicked = new Dictionary<ChatId, List<string>>();
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        // update.Message = null;
        // Only process Message updates: https://core.telegram.org/bots/api#message
        switch (update.Type)
        {
            case UpdateType.Message:
                break;

            case UpdateType.CallbackQuery:
                CallbackQueryHandle(botClient, update, cancellationToken);
                break;
        }

        // Only process messages
        if (update.Message is not { } message)
        {
            return;
        }

        switch (message.Type)
        {
            case MessageType.Text:
                await TextMessageHandle(message, botClient, cancellationToken);
                break;

        }


    }


    private async void CallbackQueryHandle(ITelegramBotClient botClient, Update update, CancellationToken token)
    {

        var queryMessage = update.CallbackQuery?.Message;

        if (queryMessage == null)
            return;

        var chatId = queryMessage.Chat.Id;

        if (!_isClicked.ContainsKey(chatId))
        {
            _isClicked[chatId] = new List<string>();
        }

        switch (_commandStatuses[chatId])
        {
            case CommandStatus.EditNote:
                if (new List<string> { "1", "2", "3", "4", "5" }.Contains(update.CallbackQuery.Data))
                {
                    break;
                }

                if (_isClicked[chatId].Contains(update.CallbackQuery.Data))
                {
                    break;
                }
                _isClicked[chatId].Add(update.CallbackQuery.Data);
                await _controller.EditNote(new BasicRequest(botClient, queryMessage, token), update.CallbackQuery.Data);

                break;

            case CommandStatus.AddPermission:
                if (_isClicked[chatId].Contains(update.CallbackQuery.Data))
                {
                   break;
                }
                _isClicked[chatId].Add(update.CallbackQuery.Data);
                await _controller.AddPermission(new BasicRequest(botClient, queryMessage, token),
                    update.CallbackQuery.Data);
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
        // var messageParts = message.Text.Split(" ");
        var messageText = message.Text;
        var chatId = message.Chat.Id;

        switch (messageText)
        {
            case "/start":
                _commandStatuses[chatId] = CommandStatus.Start;
                break;

            case "/hello":
                _commandStatuses[chatId] = CommandStatus.Hello;
                break;

            case "/create_note":
                _commandStatuses[chatId] = CommandStatus.CreateNote;
                break;

            case "/delete_note":
                _commandStatuses[chatId] = CommandStatus.DeleteNote;
                break;

            case "/edit_note":
                _commandStatuses[chatId] = CommandStatus.EditNote;
                break;

            case "/get_my_notes":
                _commandStatuses[chatId] = CommandStatus.GetMyNotes;
                break;

            case "/get_note":
                _commandStatuses[chatId] = CommandStatus.GetNote;
                break;

            case "/get_other_notes":
                _commandStatuses[chatId] = CommandStatus.GetOtherNotes;
                break;

            case "/add_permission":
                _commandStatuses[chatId] = CommandStatus.AddPermission;
                break;

            case "/delete_permission":
                _commandStatuses[chatId] = CommandStatus.DeletePermission;
                break;

            case "/get_permissions":
                _commandStatuses[chatId] = CommandStatus.GetPermissions;
                break;
        }

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        if (_commandStatuses[chatId] == CommandStatus.None)
        {
            return;
        }

        switch (_commandStatuses[chatId])
        {
            case CommandStatus.Start:
                await _controller.Start(new BasicRequest(botClient, message, cancellationToken));
                _commandStatuses[chatId] = CommandStatus.None;
                break;

            case CommandStatus.Hello:
                await _controller.SayHello(new BasicRequest(botClient, message, cancellationToken));
                _commandStatuses[chatId] = CommandStatus.None;
                break;


            case CommandStatus.CreateNote:
                var createResponse = await _controller.CreateNote(
                    new CreateNoteRequest(botClient, message, cancellationToken));

                if (createResponse.IsReady)
                {
                    _commandStatuses[chatId] = CommandStatus.None;
                }

                break;

            case CommandStatus.DeleteNote:
                var deleteResponse =
                    await _controller.DeleteNote(new BasicRequest(botClient, message, cancellationToken));

                if (deleteResponse.IsReady)
                {
                    _commandStatuses[chatId] = CommandStatus.None;
                }

                break;

            case CommandStatus.EditNote:
                var editResponse = await _controller.EditNote(new BasicRequest(botClient, message, cancellationToken));

                if (editResponse.IsReady)
                {
                    _commandStatuses[chatId] = CommandStatus.None;
                    _isClicked[chatId].Clear();
                }

                break;

            case CommandStatus.GetMyNotes:
                await _controller.GetMyNotes(new BasicRequest(botClient, message, cancellationToken));
                _commandStatuses[chatId] = CommandStatus.None;
                break;

            case CommandStatus.GetNote:
                var getNoteResponse =
                    await _controller.GetMyNote(new BasicRequest(botClient, message, cancellationToken));

                if (getNoteResponse.IsReady)
                {
                    _commandStatuses[chatId] = CommandStatus.None;
                }

                break;

            case CommandStatus.GetOtherNotes:
                await _controller.GetOtherNotes(new BasicRequest(botClient, message, cancellationToken));
                _commandStatuses[chatId] = CommandStatus.None;
                break;

            case CommandStatus.AddPermission:
                var addPermissionResponse =
                    await _controller.AddPermission(new BasicRequest(botClient, message, cancellationToken));

                if (addPermissionResponse.IsReady)
                {
                    _commandStatuses[chatId] = CommandStatus.None;
                    _isClicked[chatId].Clear();
                }

                break;

            case CommandStatus.DeletePermission:
                var deletePermissionResponse =
                    await _controller.DeletePermission(new BasicRequest(botClient, message, cancellationToken));

                if (deletePermissionResponse.IsReady)
                {
                    _commandStatuses[chatId] = CommandStatus.None;
                }

                break;

            case CommandStatus.GetPermissions:
                await _controller.GetGivePermissions(new BasicRequest(botClient, message, cancellationToken));
                _commandStatuses[chatId] = CommandStatus.None;
                break;

        }


        // Echo received message text
        // Message sentMessage = await botClient.SendTextMessageAsync(
        //     chatId: chatId,
        //     text: "You said:\n" + messageText,
        //     cancellationToken: cancellationToken);
    }
}