using System.Security.AccessControl;
using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;

namespace Notegether.Bll.Commands;

public record EditNoteCommand(
    string Identifier,
    string EditPartOfNote,
    string NewData
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
        NoteModel result = await 
            _noteService.EditNoteTitle(request.Identifier, request.NewData, request.EditPartOfNote);
        
        
        string answer = "";

        if (result == null)
        {
            answer = $"<b>Заметки с индентификатором {request.Identifier} нет!</b>";
        }
        else
        {
            answer = $"Заметка успешно изменена!\n<b>Название:</b> {result.Name}\n" +
                     $"<b>Идентификатор:</b> {request.Identifier}\n" +
                     $"<b>Описание:</b> {result.Description}\n" +
                     $"<b>Текст:</b> {result.Text}";
        }
        return new EditNoteResult(answer);

    }
}