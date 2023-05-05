using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Notegether.Dal.Entities;

public record UserEntity
{
    public long Id { get; set; }
    
    public long UserId { get; set; }
    
    public long ChatId { get; set; }
    
    public string UserName { get; set; } = string.Empty;
    
    
}