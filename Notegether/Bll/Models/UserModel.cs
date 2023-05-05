namespace Notegether.Bll.Models;

public record UserModel
{
    public string UserName { get; set; }
    public long UserId { get; set; }
    public long ChatId { get; set; }
};