using Telegram.Bot;
using Telegram.Bot.Types;

namespace Notegether.Bll.Models;

public record CreateNoteRequest(
    ITelegramBotClient BotClient,
    Message Message,
    CancellationToken CancellationToken
    );