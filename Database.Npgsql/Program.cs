using Npgsql;
// ReSharper disable ConvertToConstant.Local

// 1. Создаём объект NpgsqlDataSource - через него будут происходить обращения к базе данных.
const string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";
await using var dataSource = NpgsqlDataSource.Create(connectionString);

//
// 2. Добавляем таблицу пользователей в БД. 
// Для этого используем методы:
//     - NpgsqlDataSource.CreateCommand - создать объект команды. В аргументах передаём запрос в SQL формате.
//     - NpgsqlCommand.ExecuteNonQueryAsync - выполнить команду в БД (обычно применяется тогда, когда результат
//       выполнения запроса нам не нужен)
//
const string createTableCommandText = @"
        DROP TABLE IF EXISTS user_info; -- На всякий случай удалим таблицу, если она уже есть в БД.
        CREATE TABLE IF NOT EXISTS user_info (
            id bigserial PRIMARY KEY, 
            login VARCHAR(100) NOT NULL, 
            created_on TIMESTAMP WITH TIME ZONE NOT NULL
        )";

await using (var cmd = dataSource.CreateCommand(createTableCommandText))
{
    await cmd.ExecuteNonQueryAsync();
}

//
// 3. Вставляем запись о пользователях в БД. 
// Для этого используем уже знакомые методы CreateCommand и ExecuteNonQueryAsync,
// а также метод NpgsqlParametersCollection.Add - в него мы передаём значение, которое должно подставиться в запрос.
// В нашем примере, вместо $1 подставится слово `first_some_user`, а вместо $2 - `second_some_user`.
// Третий логин `some_third_login` мы подставили напрямую в запрос чтобы показать - что можно и так.
// Однако стоит заметить, что это плохая практика - и стоит использовать параметры всегда, когда это возможно.
//
var firstUser = "first_some_user";
var secondUser = "second_some_user";
var thirdUser = "some_third_login";

var insertRecordsCommandText = $@"
    INSERT INTO user_info(login, created_on) 
    VALUES 
        ($1, now()), 
        ($2, now()), 
        ('{thirdUser}', now());";

await using (var cmd = dataSource.CreateCommand(insertRecordsCommandText))
{
    cmd.Parameters.Add(new NpgsqlParameter { Value = firstUser });
    cmd.Parameters.Add(new NpgsqlParameter { Value = secondUser });
    await cmd.ExecuteNonQueryAsync();
}

//
// 4. Вытащим информацию о созданных пользователях.
// Для этого аналогично создадим команду, однако вместо метода ExecuteNonQueryAsync применим метод ExecuteReaderAsync,
// который позволяет вычитывать результат выполнения метода в БД.
// Записи вычитываются в цикле while (await reader.ReadAsync()) (ReadAsync вернёт false когда кончатся записи результата).
// Столбцы из записей вычитываются при помощи методов GetInt64, GetString, GetDateTime - в аргументах
// указываем позицию колонки начиная с 0. 
//

const string selectRecordsCommandText = "SELECT id, login, created_on FROM user_info";
await using var selectCmd = dataSource.CreateCommand(selectRecordsCommandText);

await using var reader = await selectCmd.ExecuteReaderAsync();
while (await reader.ReadAsync())
{
    Console.WriteLine($"Идентификатор {reader.GetInt64(0)}, " +
                      $"пользователь {reader.GetString(1)}, " +
                      $"дата создания - {reader.GetDateTime(2)}");
}