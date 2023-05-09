using Microsoft.EntityFrameworkCore;
using Notegether.Dal.Entities;
using Notegether.Dal.Queries;
using Telegram.Bot.Types;

namespace Notegether.Dal.Repositories;

public class UserRepository : IUserRepository
{

    private readonly MyDbContext _dbContext;

    public UserRepository(MyDbContext context)
    {
        _dbContext = context;
    }
    public async Task AddUser(AddUserQuery query)
    {
        await using (_dbContext)
        {
            await _dbContext.Users.AddAsync(new UserEntity
            {
                ChatId = query.ChatId,
                UserId = query.UserId,
                UserName = query.UserName
            });

            await _dbContext.SaveChangesAsync();
        }
        
    }
    public async Task<UserEntity> GetUserByUserName(string userName)
    {
       return await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userName);
    }
    public string GetUserName(long userId)
    {
        return _dbContext.Users.FirstOrDefault(x => x.ChatId == userId).UserName;
    }
}