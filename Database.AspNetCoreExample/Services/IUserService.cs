using Database.AspNetCoreExample.Controllers;
using Database.AspNetCoreExample.Models;

namespace Database.AspNetCoreExample.Services;

/// <summary>
/// Предоставляет возможности работы с пользователями.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Получить информацию о пользователе.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Информация о пользователе с идентификатором <paramref name="userId"/> в формате <see cref="UserInfo"/>.
    /// Если пользователя не существовало - возвращает <c>null</c>.
    /// </returns>
    Task<UserInfo?> GetAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить список всех пользователей.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список с информацией о всех пользователях в формате <see cref="UserInfo"/>.</returns>
    Task<IList<UserInfo>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать пользователя и получить его идентификатор.
    /// </summary>
    /// <param name="userInfo">Информация о пользователе.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task<long> CreateAsync(UserCreationInfo userInfo, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновить информацию о пользователе.
    /// </summary>
    /// <param name="userInfo">Информация о пользователе.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task UpdateAsync(UserInfo userInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить пользователя по идентификатору и вернуть информацию о нём.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Информация об удалённом пользователе с идентификатором <paramref name="userId"/>
    /// в формате <see cref="UserInfo"/>.
    /// Если пользователя не существовало - возвращает <c>null</c>.
    /// </returns>
    Task<UserInfo?> DeleteAsync(long userId, CancellationToken cancellationToken = default);
}