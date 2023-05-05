using Notegether.Bll.Models;
using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;
using Notegether.Dal.Queries;

namespace Notegether.Bll.Services;

public class UserService : IUserService
{

    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task SignInNewUser(UserModel userModel)
    {
        await _userRepository.AddUser(new AddUserQuery(userModel.UserName, userModel.UserId, userModel.ChatId));
    }
}