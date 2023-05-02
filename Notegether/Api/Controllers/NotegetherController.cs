using MediatR;

namespace Notegether.Api;

public class NotegetherController
{
    private IMediator _mediator;

    public NotegetherController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
}