using System.Text;
using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Models.Enums;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;
using Notegether.Dal.Entities;
using Telegram.Bot.Types;

namespace Notegether.Bll.Commands;

public record GetNoteCommand(
    GettingType GettingType,
    long UserId,
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
        List<NoteEntity> otherNotes = new List<NoteEntity>();

        switch (request.GettingType)
        {
            case GettingType.GetMyNotes:
                notes.AddRange( _noteService.GetMyNotes(request.UserId));
                break;
            
            case GettingType.GetOneNote:
                var note = await _noteService.GetOneNote(request.Identifier, request.UserId);

                if (note != null)
                {
                    return $"<b>Заметка:</b> \n<i>Название: </i> {note.Title}\n" +
                           $"<i>Идентификатор: {note.ShortIdentifier}</i>\n" +
                           $"<i>Описание: {note.Description}</i>\n" +
                           $"<i>Текст:\n</i> {note.Text}";
                }
                else
                {
                    return $"<b>Заметки с индентификатором</b> {request.Identifier} <b>нет или вам не выдали доступ к ней!</b>";
                }
            
            case GettingType.GetOtherNotes:
                notes.AddRange(await _noteService.GetOtherNotesReader(request.UserId));
                otherNotes.AddRange(await _noteService.GetOtherNotesRedactor(request.UserId));
                break;
        }

        var stringBuilder = new StringBuilder();

        if (notes.Count == 0  && otherNotes.Count == 0)
        {
            stringBuilder.Append(
                "У вас нет ни одной заметки!\n<i>Если хотите добавить, введите команду /create_note</i>");
        }
        else if (request.GettingType == GettingType.GetOtherNotes)
        {
            if (notes.Count > 0)
            {
                stringBuilder.Append("Заметки с уровнем доступа <b>'Читатель'</b>:\n");
                for (int i = 0; i < notes.Count; i++)
                {
                    stringBuilder.Append($"<b>{i + 1}. {notes[i].Title}</b>\n");
                    stringBuilder.Append($"<i>Идентификатор: {notes[i].ShortIdentifier}</i>\n");
                    stringBuilder.Append($"<i>Описание: {notes[i].Description}</i>\n\n");
                }
            }

            if (otherNotes.Count > 0)
            {
                stringBuilder.Append("\nЗаметки с уровнем доступа <b>'Редактор'</b>:\n");
                for (int i = 0; i < otherNotes.Count; i++)
                {
                    stringBuilder.Append($"<b>{i + 1}. {otherNotes[i].Title}</b>\n");
                    stringBuilder.Append($"<i>Идентификатор: {otherNotes[i].ShortIdentifier}</i>\n");
                    stringBuilder.Append($"<i>Описание: {otherNotes[i].Description}</i>\n\n");
                }
            }
        }
        else
        {
            stringBuilder.Append("Ваши заметки: \n");
            for (int i = 0; i < notes.Count; i++)
            {
                stringBuilder.Append($"<b>{i + 1}. {notes[i].Title}</b>\n");
                stringBuilder.Append($"<i>Идентификатор: {notes[i].ShortIdentifier}</i>\n");
                stringBuilder.Append($"<i>Описание: {notes[i].Description}</i>\n\n");
            }
        }

        var resultString = await Task.Factory.StartNew(() => stringBuilder.ToString());

        return resultString;
    }
}