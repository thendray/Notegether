using Notegether.Dal.Models.Enums;

namespace Notegether.Dal.Entities;

public record PermissionEntity
{
    public long Id { get; set; }
    public long NoteId { get; set; }
    public long GetPermissionUserChatId { get; set; }
    public PermissionStatus PermissionStatus { get; set; }
}