using Telegram.Bot;
using Telegram.Bot.Types;

namespace Notegether.Api.Requests;

public record HelloRequest(
    ITelegramBotClient BotClient,
    Message Message,
    CancellationToken CancellationToken
    );