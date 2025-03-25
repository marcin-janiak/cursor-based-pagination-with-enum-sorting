using Microsoft.EntityFrameworkCore;

namespace Host;

public class PlaygroundDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public PlaygroundDbContext(DbContextOptions<PlaygroundDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasKey(x => x.Id);
        modelBuilder.Entity<User>().Property(x => x.SomeEnum);
    }
}

public class User
{
    public Guid Id { get; set; }
    public SomeEnum SomeEnum { get; set; }
}

public enum SomeEnum
{
    None = 0,
    Foo,
    Bar,
}