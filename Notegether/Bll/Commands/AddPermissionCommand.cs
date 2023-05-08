using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;

namespace Notegether.Bll.Commands;

public record AddPermissionCommand(
    string Identifier,
    string GetUserName,
    string GiveUserName,
    long OwnerChatId,
    string PermissionStatus
    ) : IRequest<AddPermissionResult>;


public class AddPermissionCommandHandler :
    IRequestHandler<AddPermissionCommand, AddPermissionResult>
{

    private readonly IPermissionService _permissionService;

    public AddPermissionCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }
    
    public async Task<AddPermissionResult> Handle(AddPermissionCommand request, CancellationToken cancellationToken)
    {
        var userChatId = await _permissionService.GetUserChatId(request.GetUserName);
        var isNoteExist = await _permissionService.CheckNote(request.Identifier);
        
        var isUserInSystem = userChatId != 0;
        var whoGetMessage = "";
        var whoGiveMessage = "";
        string permissionRole = request.PermissionStatus == "reader" ? "Читатель" : "Редакор";

        if (!isUserInSystem)
        {
            whoGiveMessage =
                $"<i>К сожалению пользователь {request.GetUserName} пока не пользуется данным ботом, " +
                $"поэтому не получится добавить ему разрешение</i>";
        }
        else if (!isNoteExist)
        {
            whoGiveMessage = $"<i>У вас нет заметки с идентификатором {request.Identifier}!</i>";
        }
        else
        {
            whoGetMessage = $"<b>Привет, {request.GetUserName}!</b>\n" +
                            $"Пользователь {request.GiveUserName} выдал тебе доступ к заметке {request.Identifier} с уровнем <i>{permissionRole}</i>!\n" +
                            $"Детальнее ознакомиться с содержанием заметки ты можешь по команде\n /get_note или /get_other_notes";

            whoGiveMessage =
                $"Отлично, пользователь {request.GetUserName} теперь имеет доступ к заметке {request.Identifier} с уровнем {permissionRole}";
        }

        if (isUserInSystem && isNoteExist)
        {
            await _permissionService.Add(request.Identifier, request.OwnerChatId, userChatId, request.PermissionStatus);
        }

        return new AddPermissionResult(userChatId, whoGetMessage, whoGiveMessage, (isUserInSystem && isNoteExist));
    }
}