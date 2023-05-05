namespace Notegether.Dal.Queries;

public record AddUserQuery(
    string UserName,
    long UserId,
    long ChatId
    );