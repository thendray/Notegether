using Microsoft.EntityFrameworkCore;
using Notegether.Dal.Entities;

namespace Notegether.Dal;


public class MyDbContext : DbContext
{
    // private readonly Settings _settings;
    //
    // public MyDbContext(Settings settings)
    // {
    //     _settings = settings;
    // }
    //
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<NoteEntity> Notes { get; set; }
    public DbSet<PermissionEntity> Permissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // var dataBasePath = _settings.DataBasePath;
        
        optionsBuilder.UseSqlite($"Data Source=../../../Notegether.sqlite");
    }
    
    
}