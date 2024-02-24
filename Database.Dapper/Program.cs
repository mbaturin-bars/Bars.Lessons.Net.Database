using Dapper;
using Npgsql;
// ReSharper disable ConvertToConstant.Local
// ReSharper disable NotAccessedPositionalProperty.Global

//
// 1. Создаём объект NpgsqlDataSource.
// После этого создаём "открытое" соединение - для этого выполняем метод OpenConnectionAsync.
// Всю остальную работу ведём уже с объектом-соединением 'connection'.
//
const string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";
await using var dataSource = NpgsqlDataSource.Create(connectionString);
await using var connection = await dataSource.OpenConnectionAsync();

//
// 2. Добавляем таблицу пользователей в БД. 
// Для этого используем метод ExecuteAsync - который просто выполнит SQL запрос в базе данных.
//
const string createTableCommandText = @"
        DROP TABLE IF EXISTS user_info; -- На всякий случай удалим таблицу, если она уже есть в БД.
        CREATE TABLE IF NOT EXISTS user_info (
            id bigserial PRIMARY KEY, 
            login VARCHAR(100) NOT NULL, 
            created_on TIMESTAMP WITH TIME ZONE NOT NULL
        )";

await connection.ExecuteAsync(createTableCommandText);

//
// 3. Вставляем запись о пользователях в БД. 
// Для этого используем тот же метод ExecuteAsync.
// В его аргументы передаём массив пользователей - метод сам модифицирует SQL запрос,
// чтобы в конце концов были созданы несколько записей. Для того, чтобы указать методу, куда вставить параметры
// используется имена свойств помеченные @ (например @Login).
//

var users = new[]
{
    new UserCreateInfo("first_some_user"),
    new UserCreateInfo("second_some_user"),
    new UserCreateInfo("some_third_login")
};

var insertRecordsCommandText = $@"
    INSERT INTO user_info(login, created_on) 
    VALUES (@Login, now());";

await connection.ExecuteAsync(insertRecordsCommandText, users);

//
// 4. Вытащим информацию о созданных пользователях.
// Выполняем метод QueryAsync - он вернёт нам объекты, соответствующие результатам запроса в виде перечисления.
//
// Ps. Важно, чтобы имена свойств совпадали с именами колонок в запросе (без учёта регистра) - иначе значение
// не вставится в объект. Поэтому для колонки `created_on` добавлено 
//

const string selectRecordsCommandText = @"SELECT id, login, created_on as ""CreationDate"" FROM user_info";
var usersEnumerable = await connection.QueryAsync<UserInfo>(selectRecordsCommandText);
foreach (var user in usersEnumerable)
{
    Console.WriteLine($"Идентификатор {user.Id}, пользователь {user.Login}, дата создания - {user.CreationDate}");
}

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
/// <param name="Login">Логин  пользователя</param>
public sealed record UserCreateInfo(string Login);