using MediatR;

namespace Notegether.Bll.Models;

public record AddPermissionResult(
    long WhoGetChatId,
    string WhoGetMessage,
    string WhoGiveMessage,
    bool IsSuccess
    );
    