using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Services;

namespace Notegether.Bll.Commands;

public record CreateNoteCommand(
    string Name,
    string Description,
    string Text
    ) : IRequest<CreateNoteResult>;

public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, CreateNoteResult>
{
    
    private readonly INoteService _service;

    public CreateNoteCommandHandler(INoteService service)
    {
        _service = service;
    }

    public Task<CreateNoteResult> Handle(
        CreateNoteCommand request,
        CancellationToken cancellationToken)
    {

        return null;

    }
}