using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Services;

namespace Notegether.Bll.Commands;

public record SayHelloCommand(
    string UserName
    ) : IRequest<SayHelloResult>;

public class SayHelloCommandHandler : IRequestHandler<SayHelloCommand, SayHelloResult>
{

    private readonly ISayHelloService _service;

    public SayHelloCommandHandler(ISayHelloService service)
    {
        _service = service;
    }

    public async Task<SayHelloResult> Handle(SayHelloCommand request, CancellationToken cancellationToken)
    {
        var result = await Task.Factory.StartNew( 
            () => 
                new SayHelloResult($"{_service.Hello()} {request.UserName}!\n")
            );

        return result;
    }
}