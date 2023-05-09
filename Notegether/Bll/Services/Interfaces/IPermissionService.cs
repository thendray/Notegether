using Notegether.Dal.Entities;

namespace Notegether.Bll.Services.Interfaces;

public interface IPermissionService
{

    public Task<long> GetUserChatId(string requestGetUserName);
    public Task<bool> CheckNote(string requestIdentifier);
    public Task Add(string requestIdentifier, long requestOwnerChatId, long userChatId, string requestPermissionStatus);
    public Task<Tuple<PermissionEntity, NoteEntity>> DeletePermission(string requestIdentifier, string requestUserName);
    public IEnumerable<PermissionEntity> GetPermissionsByCreatorId(long requestWhoGiveChatId);
    public string GetUserName(long whoGetChatId);
    public Task<string> GetNoteName(string noteIdentifier);
}