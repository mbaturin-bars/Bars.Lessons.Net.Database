using Database.AspNetCoreExample.Controllers;
using Database.AspNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.AspNetCoreExample.Services.EntityFramework;

/// <summary>
/// Сервис, позволяющий работать с пользователями в базе данных (реализация через EntityFramework).
/// </summary>
public class EntityFrameworkUserService : IUserService
{
    private readonly UserDbContext _dbContext;

    /// <summary>
    /// Создать экземпляр типа <see cref="EntityFrameworkUserService"/>.
    /// </summary>
    public EntityFrameworkUserService(UserDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<UserInfo?> GetAsync(long userId, CancellationToken cancellationToken = default)
        => await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId,
            cancellationToken);

    /// <inheritdoc />
    public async Task<IList<UserInfo>> GetListAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Users.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<long> CreateAsync(UserCreationInfo userInfo, CancellationToken cancellationToken = default)
    {
        var entry = _dbContext.Users.Add(new UserInfo { Login = userInfo.Login });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entry.Entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(userInfo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserInfo?> DeleteAsync(long userId, CancellationToken cancellationToken = default)
    {
        var entity = await GetAsync(userId, cancellationToken);

        if (entity != null)
        {
            await _dbContext.Users
                .Where(x => x.Id == userId)
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);
        }

        return entity;
    }
}