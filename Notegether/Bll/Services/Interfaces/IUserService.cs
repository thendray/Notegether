using Notegether.Bll.Models;

namespace Notegether.Bll.Services.Interfaces;

public interface IUserService
{

    public Task SignInNewUser(UserModel userModel);
}