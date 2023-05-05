using MediatR;
using Notegether.Bll.Models;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;

namespace Notegether.Bll.Commands;

public record StartCommand(
    string UserName,
    long ChatId,
    long UserId
    ) : IRequest;


public class StartCommandHandler : IRequestHandler<StartCommand>
{

    private readonly IUserService _userService;

    public StartCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task Handle(StartCommand request, CancellationToken cancellationToken)
    {
       await _userService.SignInNewUser(new UserModel
        {
            UserName = request.UserName,
            ChatId = request.ChatId,
            UserId = request.UserId
        });
        
    }
}