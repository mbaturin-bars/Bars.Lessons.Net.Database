using Database.EntityFramework;
using Microsoft.EntityFrameworkCore;

// 0. В рамках данного кода мы уже не создаём таблицу в БД - её за нас создаёт 'миграция'.
// Для генерации класса миграции мы выполняем команду: dotnet ef migrations add SOME_MIGRATION_NAME
// Для проведения миграции мы выполняем команду: dotnet ef database update 

// 1. Создаём экземпляр класса ApplicationDbContext. В будущем все обращения к БД будут происходить через его свойства.
await using var db = new ApplicationDbContext();

// 2. Создаём несколько пользователей - для этого используем методы:
//     - db.Users.AddRangeAsync - добавить данных пользователей в отслеживание.
//     - db.SaveChangesAsync - выполнить все предыдущие операции - в нашем случае добавить пользователей в БД.
var users = new[]
{
    new UserInfo { Login = "first_some_user" },
    new UserInfo { Login = "second_some_user" },
    new UserInfo { Login = "some_third_login" },
};
await db.Users.AddRangeAsync(users);
await db.SaveChangesAsync();

// 3. Для получения списка пользователей вызываем метод ToListAsync().
// Если нам нужна какая либо фильтрация или др. - мы можем применять методы LINQ - Where, OrderBy
// и др. к свойству db.Users.
var usersList = await db.Users.ToListAsync();
foreach (var user in usersList)
{
    Console.WriteLine($"Идентификатор {user.Id}, пользователь {user.Login}, дата создания - {user.CreationDate}");
}