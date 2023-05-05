using Telegram.Bot;
using Telegram.Bot.Types;

namespace Notegether.Api.Requests;

public record BasicRequest(
    ITelegramBotClient BotClient,
    Message Message,
    CancellationToken CancellationToken
    );