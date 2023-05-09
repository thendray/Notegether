using System.Security.AccessControl;
using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Models.Enums;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;

namespace Notegether.Bll.Commands;

public record EditNoteCommand(
    string Identifier,
    string EditPartOfNote,
    string NewData,
    long UserId,
    string UserName,
    string EditType
    ) : IRequest<EditNoteResult>;

public class EditNoteCommandHandler :
    IRequestHandler<EditNoteCommand, EditNoteResult>
{

    private readonly INoteService _noteService;

    public EditNoteCommandHandler(INoteService noteService)
    {
        _noteService = noteService;
    }

    public async Task<EditNoteResult> Handle(EditNoteCommand request, CancellationToken cancellationToken)
    {

        EditType editType = request.EditType == "rewrite" ? EditType.Rewrite : EditType.AddText;
        
        NoteModel result = await
            _noteService.EditNote(request.Identifier,
                request.UserId,
                request.NewData,
                request.EditPartOfNote,
                editType);


        string answer = "";
        List<long> othersChatIds = new List<long>();

        if (result == null)
        {
            answer = $"<b>У вас нет заметки с индентификатором</b> {request.Identifier}!";
            return new EditNoteResult(answer, "", othersChatIds);
        }

        if (result.Text == "" && result.Name == "" && result.Description == "")
        {
            answer = "<i>У вас нет доступа к этой заметки!</i>";
            return new EditNoteResult(answer, "", othersChatIds);
        }

        answer = $"Заметка успешно изменена!\n<b>Название:</b> {result.Name}\n" +
                 $"<b>Идентификатор:</b> {request.Identifier}\n" +
                 $"<b>Описание:</b> {result.Description}\n" +
                 $"<b>Текст:\n</b> {result.Text}";

        othersChatIds = _noteService.GetChatIdsWithPermissions(request.Identifier, request.UserId);

        string messageForOthers = $"Привет, пользователь <b>{request.UserName}</b> " +
                                  $"внес изменения в заметку <b>'{result.Name}'</b> с идентификатором <b>{request.Identifier}</b>!\n" +
                                  $"<i>Подробнее с содержанием заметки вы можете ознакомиться по команде /get_note</i>";

        return new EditNoteResult(answer, messageForOthers, othersChatIds);

    }
}