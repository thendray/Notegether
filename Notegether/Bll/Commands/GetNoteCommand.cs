using System.Text;
using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Models.Enums;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;
using Notegether.Dal.Entities;

namespace Notegether.Bll.Commands;

public record GetNoteCommand(
    GettingType GettingType,
    long CreatorId,
    string Identifier
    ) : IRequest<string>;


public class GetNoteCommandHandler : IRequestHandler<GetNoteCommand, string>
{

    private readonly INoteService _noteService;

    public GetNoteCommandHandler(INoteService service)
    {
        _noteService = service;
    }

    public async Task<string> Handle(GetNoteCommand request, CancellationToken cancellationToken)
    {
        List<NoteEntity> notes = new List<NoteEntity>();
        
        switch (request.GettingType)
        {
            case GettingType.GetMyNotes:
                notes.AddRange( _noteService.GetMyNotes(request.CreatorId));
                break;
            
            case GettingType.GetOneNote:
                var note = await _noteService.GetOneNotes(request.Identifier);
                return $"<b>Ваша заметка:</b> \n<i>Название: </i> {note.Title}\n" +
                       $"<i>Идентификатор: {note.ShortIdentifier}</i>\n" +
                       $"<i>Описание: {note.Description}</i>\n" +
                       $"<i>Текст: {note.Text}</i>";
                
        }

        var stringBuilder = new StringBuilder();

        if (notes.Count == 0)
        {
            stringBuilder.Append(
                "У вас нет ни одной заметки!\n<i>Если хотите добавить, введите команду /create_note</i>");
        }
        else
        {
            stringBuilder.Append("Ваши заметки: \n");
        }
        for (int i = 0; i < notes.Count; i++)
        {
            stringBuilder.Append($"<b>{i + 1}. {notes[i].Title}</b>\n");
            stringBuilder.Append($"<i>Идентификатор: {notes[i].ShortIdentifier}</i>\n");
            stringBuilder.Append($"<i>Описание: {notes[i].Description}</i>\n\n");
        }

        var resultString = await Task.Factory.StartNew(() => stringBuilder.ToString());

        return resultString;
    }
}