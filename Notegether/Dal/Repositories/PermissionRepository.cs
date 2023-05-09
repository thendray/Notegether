using Microsoft.EntityFrameworkCore;
using Notegether.Bll.Models;
using Notegether.Dal.Entities;

namespace Notegether.Dal.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly MyDbContext _dbContext;

    public PermissionRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(PermissionEntity permissionEntity)
    {
        var permission = _dbContext.Permissions.FirstOrDefault(x
            => x.NoteIdentifier == permissionEntity.NoteIdentifier && x.WhoGetChatId == permissionEntity.WhoGetChatId);

        if (permission == null)
        {
            _dbContext.Permissions.Add(permissionEntity);
        }
        else
        {
            permission.PermissionStatus = permissionEntity.PermissionStatus;
            await Update(permission.NoteIdentifier, permission.WhoGetChatId, permission);
        }
        await _dbContext.SaveChangesAsync();
    }
    public async Task Update(string noteIdentifier, long whoGetId, PermissionEntity permissionEntity)
    {
        var oldEntity = await _dbContext.Permissions.FirstOrDefaultAsync(x 
            => x.NoteIdentifier == noteIdentifier && x.WhoGetChatId == whoGetId);

        if (oldEntity != null)
        {
            _dbContext.Entry(oldEntity).CurrentValues.SetValues(permissionEntity);
            _dbContext.SaveChanges();
        }
    }
    public async Task<PermissionEntity> Get(string identifier, long id)
    {
        return await _dbContext.Permissions.FirstOrDefaultAsync(x
            => x.NoteIdentifier == identifier && x.WhoGetChatId == id);
    }
    
    public IEnumerable<PermissionEntity> GetAllGotNotes(long userId)
    {
        foreach (var permission in _dbContext.Permissions)
        {
            if (permission.WhoGetChatId == userId)
            {
                yield return permission;
            }
        }
    }
    public void Delete(string identifier, long userId)
    {
        var permissionForDel =  _dbContext.Permissions.FirstOrDefault(x 
            => x.NoteIdentifier == identifier && x.WhoGetChatId == userId);
        
        if (permissionForDel != null)
        {
            _dbContext.Permissions.Remove(permissionForDel);

            _dbContext.SaveChanges();
            
        }
        
    }
    public IEnumerable<PermissionEntity> GetAllByIdentifier(string identifier)
    {
        foreach (var permission in _dbContext.Permissions)
        {
            if (permission.NoteIdentifier == identifier)
            {
                yield return permission;
            }
        }
    }
    public IEnumerable<PermissionEntity> GetAllGivePermissions(long chatId)
    {
        return _dbContext.Permissions.Where(x => x.WhoGiveChatId == chatId);
    }

}