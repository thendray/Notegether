using System.Data.Common;
using MediatR;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal.Entities;
using Notegether.Dal.Models.Enums;

namespace Notegether.Bll.Commands;

public record DeletePermissionCommand(
    string Identifier,
    string UserName,
    long ChatId
    ) : IRequest<string>;


public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, string>
{

    private readonly IPermissionService _permissionService;

    public DeletePermissionCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<string> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {

        var result = 
            await _permissionService.DeletePermission(request.Identifier, request.UserName);

        var answer = "";

        if (result.Item2 == null && result.Item1 == null)
        {
            answer = $"Пользователь с именем <i>{request.UserName}</i> не зарегестрирован и не пользуеься ботом!";
        }

        else if (result.Item1 == null)
        {
            answer = $"У пользователя <b>{request.UserName}</b> нет доступа" +
                     $" к заметке с идентификатором {request.Identifier}";
            
        }
        
        else if (result.Item1.WhoGiveChatId != request.ChatId)
        {
            answer = $"У вас нет заметки с идентифекатором <i>{request.Identifier}</i>";
        }
        
        else
        {
            string status = result.Item1.PermissionStatus == PermissionStatus.Reader ? "Читатель" : "Редактор";
            answer = $"У пользователя {request.UserName} больше нет " +
                     $"доступа {status} к заметке с идентификатором {request.Identifier}";
        }

        return answer;
    }
}
