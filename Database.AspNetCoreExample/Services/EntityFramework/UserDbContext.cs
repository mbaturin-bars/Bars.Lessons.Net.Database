using Database.AspNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CS8618

namespace Database.AspNetCoreExample.Services.EntityFramework;

/// <summary>
/// Контекст взаимодействия с базой данных пользователей.
/// </summary>
public class UserDbContext : DbContext
{
    private readonly string _connectionString;

    /// <summary>
    /// Создать экземпляр типа <see cref="UserDbContext"/>.
    /// </summary>
    /// <param name="connectionString">Строка подключения к базе данных.</param>
    public UserDbContext(string connectionString) => _connectionString = connectionString;

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
        => optionsBuilder.UseNpgsql(_connectionString);
}