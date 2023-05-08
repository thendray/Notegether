using Notegether.Dal.Entities;
using Notegether.Dal.Models.Enums;

namespace Notegether.Dal;

public interface IPermissionRepository
{

    public Task Add(PermissionEntity permissionEntity);
    public Task Update(string noteIdentifier, long whoGetId, PermissionEntity permissionEntity);
    public Task<PermissionEntity> Get(string identifier, long id);
}