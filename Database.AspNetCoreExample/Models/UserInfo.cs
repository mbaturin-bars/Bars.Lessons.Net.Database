using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#pragma warning disable CS8618

namespace Database.AspNetCoreExample.Models;

/// <summary>
/// Информация о пользователе.
/// </summary>
[Table("user_info", Schema = "public")]
public sealed class UserInfo
{
    /// <summary>Идентификтаор пользователя.</summary>
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>Логин пользователя.</summary>
    [Column("login", TypeName = "varchar(100)"), Required]
    public string Login { get; set; }

    /// <summary>Дата\время создания пользователя.</summary>
    [Column("created_on"), Required]
    public DateTime CreationDate { get; set; }
}
