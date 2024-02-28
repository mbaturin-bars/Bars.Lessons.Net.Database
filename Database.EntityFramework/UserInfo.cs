using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.EntityFramework;

/// <summary>
/// Информация о пользователе.
/// </summary>
[Table("user_info", Schema = "public")]
public class UserInfo
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Логин пользователя.
    /// </summary>
    [Column("login", TypeName = "varchar(100)"), Required]
    public string Login { get; set; }

    /// <summary>
    /// Дата создания.
    /// </summary>
    [Column("created_on"), Required]
    public DateTime CreationDate { get; set; }
}