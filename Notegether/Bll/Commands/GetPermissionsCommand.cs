using System.Text;
using System.Xml.Linq;
using MediatR;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;
using Notegether.Dal.Models.Enums;

namespace Notegether.Bll.Commands;

public record GetPermissionsCommand(
    long WhoGiveChatId
    ) : IRequest<string>;


public class GetPermissionsCommandHandler : 
    IRequestHandler<GetPermissionsCommand, string>
{
    private readonly IPermissionService _permissionService;

    public GetPermissionsCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<string> Handle(GetPermissionsCommand request, CancellationToken cancellationToken)
    {
        var permissions = 
            _permissionService.GetPermissionsByCreatorId(request.WhoGiveChatId).ToList();

        if (permissions.Count == 0)
        {
            return "<i>Вы не выдавали ни кому доступ к вашим заметкам!</i>";
        }

        var stringBuilder = new StringBuilder();


        var readers = permissions.Where(x => x.PermissionStatus == PermissionStatus.Reader)
            .ToList();
        var redactors = permissions.Where(x => x.PermissionStatus == PermissionStatus.Redactor)
            .ToList();

        if (readers.Count > 0)
        {
            stringBuilder.Append("<b>Разрешение 'Читатель':</b>\n");
            
            for (int i = 0; i < readers.Count; i++)
            {
                string userName = _permissionService.GetUserName(readers[i].WhoGetChatId);
                string noteName = await _permissionService.GetNoteName(readers[i].NoteIdentifier);

                stringBuilder.Append($"<b>{i + 1}. {userName}</b>\n");
                stringBuilder.Append($"<i>Идентификатор заметки: {readers[i].NoteIdentifier}</i>\n");
                stringBuilder.Append($"<i>Название: {noteName}</i>\n\n");
            }
        }
        
        if (redactors.Count > 0)
        {
            stringBuilder.Append("<b>Разрешение 'Редактор':</b>\n");
            
            for (int i = 0; i < redactors.Count; i++)
            {
                string userName = _permissionService.GetUserName(redactors[i].WhoGetChatId);
                string noteName = await _permissionService.GetNoteName(redactors[i].NoteIdentifier);

                stringBuilder.Append($"<b>{i + 1}. {userName}</b>\n");
                stringBuilder.Append($"<i>Идентификатор заметки: {redactors[i].NoteIdentifier}</i>\n");
                stringBuilder.Append($"<i>Название: {noteName}</i>\n\n");
            }
        }

        return stringBuilder.ToString();
    }
}