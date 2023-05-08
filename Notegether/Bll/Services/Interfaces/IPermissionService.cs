namespace Notegether.Bll.Services.Interfaces;

public interface IPermissionService
{

    public Task<long> GetUserChatId(string requestGetUserName);
    public Task<bool> CheckNote(string requestIdentifier);
    public Task Add(string requestIdentifier, long requestOwnerChatId, long userChatId, string requestPermissionStatus);
}