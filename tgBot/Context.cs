using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace tgBot;

public class Context: DbContext
{
    public List<User> Users { get; set; }

    public Context(DbContextOptions<Context> options): base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(u => u.Id);
    }
}