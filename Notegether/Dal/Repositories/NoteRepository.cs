using Notegether.Dal.Entities;
using Telegram.Bot.Types;

namespace Notegether.Dal.Repositories;

public class NoteRepository : INoteRepository
{

    private readonly MyDbContext _dbContext;

    public NoteRepository(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddUser(string name)
    {
        _dbContext.Users.Add(new UserEntity()
        {
            Id = 12234,
            Name = name,
            Description = "-",
            Text = "---"
        });

        _dbContext.SaveChanges();
    }
    
}