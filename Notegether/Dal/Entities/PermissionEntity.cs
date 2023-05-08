using Notegether.Dal.Models.Enums;

namespace Notegether.Dal.Entities;

public record PermissionEntity
{
    public long Id { get; set; }
    public string NoteIdentifier { get; set; }
    public long WhoGiveChatId { get; set; }
    public long WhoGetChatId { get; set; }
    public PermissionStatus PermissionStatus { get; set; }
}