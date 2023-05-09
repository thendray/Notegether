using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Services.Interfaces;

namespace Notegether.Bll.Commands;

public record DeleteNoteCommand(
    string Identifier,
    long CreatorId
    ) : IRequest<DeleteNoteResult>;


public class DeleteNoteCommandHandle :
    IRequestHandler<DeleteNoteCommand, DeleteNoteResult>
{

    private readonly INoteService _noteService;

    public DeleteNoteCommandHandle(INoteService noteService)
    {
        _noteService = noteService;
    }

    public async Task<DeleteNoteResult> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        string result = await _noteService.DeleteNote(request.Identifier, request.CreatorId);

        if (result == null)
        {
            return new DeleteNoteResult("<i>У вас нет доступа к этой заметки!</i>");
        }

        if (result == "")
        {
            return new DeleteNoteResult("<i>Такой заметки у вас нет!</i>");
        }

        return new DeleteNoteResult($"Готово, заметка <b>{result}</b> удалена!");
    }
}