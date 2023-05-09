using Notegether.Dal.Entities;
using Notegether.Dal.Models.Enums;

namespace Notegether.Dal;

public interface IPermissionRepository
{

    public Task Add(PermissionEntity permissionEntity);
    public Task Update(string noteIdentifier, long whoGetId, PermissionEntity permissionEntity);
    public Task<PermissionEntity> Get(string identifier, long id);
    IEnumerable<PermissionEntity> GetAllGotNotes(long userId);
    public void Delete(string identifier, long userId);
    public IEnumerable<PermissionEntity> GetAllByIdentifier(string identifier);
    public IEnumerable<PermissionEntity> GetAllGivePermissions(long chatId);
}