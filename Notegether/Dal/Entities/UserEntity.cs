using Microsoft.EntityFrameworkCore;

namespace Notegether.Dal.Entities;

public record UserEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}