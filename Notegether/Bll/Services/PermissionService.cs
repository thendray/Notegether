using Notegether.Bll.Services.Interfaces;
using Notegether.Dal;
using Notegether.Dal.Entities;
using Notegether.Dal.Models.Enums;

namespace Notegether.Bll.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserRepository _userRepository;
    private readonly INoteRepository _noteRepository;

    public PermissionService(
        IPermissionRepository permissionRepository,
        IUserRepository userRepository,
        INoteRepository noteRepository)
    {
        _permissionRepository = permissionRepository;
        _userRepository = userRepository;
        _noteRepository = noteRepository;
    }

    public async Task<long> GetUserChatId(string userName)
    {

        var user = await _userRepository.GetUserByUserName(userName);

        if (user == null)
        {
            return 0;
        }

        return user.UserId;

    }
    public async Task<bool> CheckNote(string identifier)
    {
        var note = await _noteRepository.Get(identifier);

        if (note == null)
        {
            return false;
        }

        return true;

    }
    public async Task Add(string identifier, long ownerChatId, long chatId, string permissionStatus)
    {
        var status = permissionStatus == "reader" ? PermissionStatus.Reader : PermissionStatus.Redactor;
        var permissionEntity = new PermissionEntity
        {
            NoteIdentifier = identifier,
            WhoGiveChatId = ownerChatId,
            WhoGetChatId = chatId,
            PermissionStatus = status
        };
        await _permissionRepository.Add(permissionEntity);
    }
    public async Task<Tuple<PermissionEntity, NoteEntity>> DeletePermission(string identifier, string userName)
    {
        var user = await _userRepository.GetUserByUserName(userName);
        var note = await _noteRepository.Get(identifier);

        if (user == null)
        {
            return new Tuple<PermissionEntity, NoteEntity>(null, null);
        }

        var permission = await _permissionRepository.Get(identifier, user.ChatId);

        if (permission == null)
        {
            return new Tuple<PermissionEntity, NoteEntity>(null, note);
        }

        _permissionRepository.Delete(identifier, user.ChatId);

        return new Tuple<PermissionEntity, NoteEntity>(permission, note);
    }
}