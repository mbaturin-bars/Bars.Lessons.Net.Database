using Microsoft.EntityFrameworkCore;

namespace Database.EntityFramework;

/// <summary>
/// Контекст БД в приложении.
/// </summary>
public class ApplicationDbContext : DbContext
{
    private const string ConnectionString =
        "Host=localhost;Port=5433;Username=postgres;Password=postgres;Database=postgres";

    /// <summary>
    /// DbSet для операций над пользователями.
    /// </summary>
    public DbSet<UserInfo> Users { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<UserInfo>()
            .Property(u => u.CreationDate)
            .HasDefaultValueSql("now()");

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(ConnectionString);
}