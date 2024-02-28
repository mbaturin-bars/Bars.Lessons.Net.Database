using System.ComponentModel.DataAnnotations;
using Database.AspNetCoreExample.Services;
using Microsoft.AspNetCore.Mvc;

namespace Database.AspNetCoreExample.Controllers;

/// <summary>
/// Информация о пользователе.
/// </summary>
/// <param name="Id"></param>
/// <param name="Login">Логин пользователя.</param>
/// <param name="CreationDate">Дата\время создания пользователя.</param>
public sealed record UserInfo(long Id, string Login, DateTime CreationDate);

/// <summary>
/// Информация о создаваемом пользователе.
/// </summary>
/// <param name="Login">Логин пользователя.</param>
public sealed record UserCreationInfo(string Login);

/// <summary>
/// Объединяет конечные точки, предназначенные для работы с пользователями.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService) => _userService = userService;

    /// <summary>
    /// Получить список всех пользователей.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    [HttpGet]
    [Route("list")]
    [ProducesResponseType(typeof(IList<UserInfo>), StatusCodes.Status200OK)]
    public async Task<IList<UserInfo>> GetListAsync(CancellationToken cancellationToken)
        => await _userService.GetListAsync(cancellationToken);

    /// <summary>
    /// Получить информацию о пользователе.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(long), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetAsync([FromRoute, Required] long userId, CancellationToken cancellationToken)
    {
        var userInfo = await _userService.GetAsync(userId, cancellationToken);
        return userInfo != null
            ? Ok(userInfo)
            : NotFound(userId);
    }

    /// <summary>
    /// Создать пользователя.
    /// </summary>
    /// <param name="userInfo">Информация о создаваемом пользователе.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    public async Task<long> CreateAsync([FromBody, Required] UserCreationInfo userInfo, CancellationToken cancellationToken)
        => await _userService.CreateAsync(userInfo, cancellationToken);

    /// <summary>
    /// Обновить информацию о пользователе.
    /// </summary>
    /// <param name="userInfo"></param>
    /// <param name="cancellationToken"></param>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task UpdateAsync([FromBody, Required] UserInfo userInfo, CancellationToken cancellationToken)
        => await _userService.UpdateAsync(userInfo, cancellationToken);

    /// <summary>
    /// Удалить пользователя по идентификатору и вернуть информацию о нём.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    [HttpDelete("{userId}")]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(long), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync([FromRoute, Required] long userId, CancellationToken cancellationToken)
    {
        var deletedUserInfo = await _userService.DeleteAsync(userId, cancellationToken);
        return deletedUserInfo != null
            ? Ok(deletedUserInfo)
            : NotFound(userId);
    }
}