# Взаимодействие с реляционными базами данных в NET приложении

## Введение

На данный момент большинство наших приложений так или иначе должно хранить какие либо данные.  
Это могут быть:

- системные данные - информация о пользователях системы (логины, имена и др), данные для авторизации и т.п.;
- справочные данные, которые необходимы для работы системы;
- любые данные, формируемые в результате работы приложения (для расчётных систем - результаты расчётов и т.п.);

и другие.

Хранить данные можно разными путями - в памяти приложения, напрямую в файловой системе, а также при помощи систем
управления базами данных - реляционных (SQL - PostgreSQL, MySQL и др.) и нереляционных (NoSQL - MongoDB, Redis,
Clickhouse и другие).

Большинство корпоративных систем (например, банковские системы) используют реляционные базы данных в качестве основного
хранилища, а нереляционные - в качестве аналитических баз данных, кэша или т.п.

В рамках этого занятия мы покажем, как можно взаимодействовать с БД из NET приложения.

## ADO.NET

ADO.NET (ActiveX Data Objects for .NET) - это набор библиотек, позволяющий организовать доступ NET приложений к
различным источникам данных (базы данных MSSQL, MySQL, xml документы и др.).

ADO NET предоставляет основные подходы и абстракции по организации взаимодействия с базой данных – в NET вы можете
самостоятельно написать свою реализацию ADO.NET провайдера к любому источнику данных.

> Впервые ADO.NET был добавлен ещё с NETFramework 2.0 (2002 год). На данный момент ADO.NET имеет некоторое количество
> явно устаревших подходов (например - достаточно малое количество приложений сейчас работает с сущностями DataSet и
> DataTablе), однако базовые понятия в нём ещё актуальны.

Основными компонентами в ADO.NET являются:

- Connection (соединение) - соединение с источником данных (базой данных, файлом и т.п.)
- Command (команда) - запрос к источнику данных
- Transaction - транзакция базы данных (грубо говоря - объединение команд в набор, который должны выполниться либо
  полностью, либо не выполниться ни одна из команд)
- DataReader - объект, позволяющий читать результаты запроса в базе данных

> Ps. Некоторые компоненты намерено не упоминаются (например DataSet, DataTable), потому что работа с ними зачастую либо
> не ведётся (устаревшие подходы), либо ведётся "под капотом" других более высокоуровневых библиотек. Если вы по какой
> то причине захотели изучить весь ADO.NET, обратитесь к книгам или к официальной
> документации https://learn.microsoft.com/ru-ru/dotnet/framework/data/adonet/

### Взаимодействие с PostgreSQL при помощи Npgsql

В стандартную библиотеку включены только коннекторы:
- Miscosoft SQL Server;
- Универсальный коннектор OLEDB\ODBC;
- Oracle RDBMS.

Для подключения к базам данных PostgreSQL в подавляющем большинстве приложений используется библиотека Npgsql - это
опенсорсный ADO.NET провадйер для подключения к PostgreSQL.

> Исходный код находится в репозитории https://github.com/npgsql/npgsql  
> Документация на английском языке находится на оф. сайте https://www.npgsql.org/doc/index.html

Попробуем создать приложение и подключить его к базе данных PostgreSQL, для этого:

- Создадим новое консольное приложение - через интерфейс или командой
  ```shell
  dotnet new console --name Database.Npgsql --framework net8.0
  ```

- Добавим зависимость от пакета Npgsql 8.0.2 (или любой актуальной) - через интерфейс или командой
  ```shell
  dotnet add package Npgsql --version 8.0.2
  ```

- В класс `Program.cs` напишем следующий код

```csharp
using Npgsql;

// 1.
const string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";
await using var dataSource = NpgsqlDataSource.Create(connectionString);

// 2.
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

// 3.
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


// 4.
const string selectRecordsCommandText = "SELECT id, login, created_on FROM user_info";
await using var selectCmd = dataSource.CreateCommand(selectRecordsCommandText);

await using var reader = await selectCmd.ExecuteReaderAsync();
while (await reader.ReadAsync())
{
    Console.WriteLine($"Идентификатор {reader.GetInt64(0)}, " +
                      $"пользователь {reader.GetString(1)}, " +
                      $"дата создания - {reader.GetDateTime(2)}");
}
```

Разберём написанное по пунктам:

1. Инициируем подключение к базе данных при помощи метода `NpgsqlDataSource.Create`
2. Добавляем таблицу пользователей в базу данных. Для этого:
    - Создаём команду к БД при помощи метода NpgsqlDataSource.CreateCommand. В аргументах передаём текст SQL запроса.
    - Выполняем команду в БД при помощи метода NpgsqlCommand.ExecuteNonQueryAsync.
3. Выполняем вставку записей в базу данных.   
   Кроме вышеописанных методов используется также `NpgsqlCommand.Parameters.Add` - метод вставляет указанное значение в
   запрос. Значение можно вставить также внутрь запроса напрямую (например, как указано значение _some_third_login_,
   однако параметры работают эффективнее и защищают от SQL-инъекций).
4. Запрашиваем данные из базы данных при помощи запроса `SELECT`. Используются следующие методы:
    - `ExecuteReaderAsync` - выполняет команду в базе данных и возвращает объект `NpgsqlDataReader`, который необходим
      для получения результатов запроса.
    - `ReadAsync` - перемещает указатель к следующей записи результатов. Возвращает true или false в зависимости от
      того,
      существует ли запись.
    - `GetInt64`, `GetString`, `GetDateTime` - используются для получения значения из столбца текущей записи с
      определённым типом. Число в аргументе - порядковый номер столбца начиная с 0.

> Примечание:
> 1. Описание параметров строки подключения приводится на оф. сайте документации
     Npgsql https://www.npgsql.org/doc/connection-string-parameters.html.  
     `NpgsqlDataSource.Create` - это новый способ работы с подключениями к БД.  
     В версиях до Npgsql 7.0 вы должны использовать класс `NpgsqlConnection` для работы с
     БД. Подробнее в https://www.npgsql.org/doc/basic-usage.html#connections-without-a-data-source
> 2. Методы `CreateCommand` и `ExecuteNonQueryAsync` являются реализациями стандартных методов ADO.NET, см
> 3. Подробнее о параметрах Npgsql в документации https://www.npgsql.org/doc/basic-usage.html#parameters
> 4. `ExecuteReaderAsync`, `ReadAsync` являются реализациями стандартных методов ADO.NET,
     см https://learn.microsoft.com/ru-ru/dotnet/api/system.data.common.dbcommand?view=net-8.0
     и https://learn.microsoft.com/ru-ru/dotnet/api/system.data.common.dbdatareader?view=net-8.0

Мы подключились к БД PostgreSQL и даже смогли провести несколько запросов. Однако, несмотря на то, что запросы были сами
по себе простые - код получился достаточно "многословный". Например на то, чтобы достать записи пользователей из БД нам
потребовалось как минимум выполнить команду, запустить цикл для вычитывания записей и прописать порядок чтения строки.

Хотелось бы в повседневной разработке использовать более простое решение, которое не уступало бы по
производительности.

### Dapper

Dapper - это библиотека, которая расширяет возможности ADO.NET соединений с помощью собственных методов расширений
для `IDbConnection`. Она представляет довольно простой и удобный API для выполнения SQL запросов, не зависимый от типа
реляционной базы данных.

Перепишем наш код с использованием Dapper в новом проекте, для этого:

- Создадим новое консольное приложение - через интерфейс или командой
  ```shell
  dotnet new console --name Database.Dapper --framework net8.0
  ```

- Добавим зависимости от пакетов Dapper 2.1.28 и Npgsql 8.0.2 (или любых актуальных) - через интерфейс или командой
  ```shell
  dotnet add package Dapper --version 2.1.28 && dotnet add package Npgsql --version 8.0.2
  ```

- В класс `Program.cs` напишем следующий код

```csharp
using Dapper;
using Npgsql;

// 1.
const string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";
await using var dataSource = NpgsqlDataSource.Create(connectionString);
await using var connection = await dataSource.OpenConnectionAsync();

// 2.
const string createTableCommandText = @"
        DROP TABLE IF EXISTS user_info;
        CREATE TABLE IF NOT EXISTS user_info (
            id bigserial PRIMARY KEY, 
            login VARCHAR(100) NOT NULL, 
            created_on TIMESTAMP WITH TIME ZONE NOT NULL
        )";

await connection.ExecuteAsync(createTableCommandText);

// 3.
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

// 4.
const string selectRecordsCommandText = @"SELECT id, login, created_on as ""CreationDate"" FROM user_info";
var usersEnumerable = await connection.QueryAsync<UserInfo>(selectRecordsCommandText);
foreach (var user in usersEnumerable)
{
    Console.WriteLine($"Идентификатор {user.Id}, пользователь {user.Login}, дата создания - {user.CreationDate}");
}

public record UserInfo(long Id, string Login, DateTime CreationDate): UserCreateInfo(Login);
public record UserCreateInfo(string Login);
```

Код выполнения запросов значительно упростился, давайте разберём его:

1. Мы всё так же создаём объект NpgsqlDataSource. Однако, вместо него дальше в коде мы начинаем использовать объект
   NpgsqlConnection - так как Dapper предоставляет расширения именно соединения, а не DataSource.
2. Любые запросы, не требующие возврата результата могут быть выполнены при помощи метода `ExecuteAsync` - в данном
   случае мы передаём в него SQL текст запроса.
3. Для вставки записей также мы можем использовать `ExecuteAsync`. В данном случае мы используем механизм параметризации
   запроса:
    1. В SQL запросе мы указываем место, в которое необходимо поместить параметризируемое значение. В нашем случае
       это `@Login`.
    2. В аргумент `object? parameters` метода `ExecuteAsync` мы передаём параметры - в нашем случае это массив
       пользователей. Dapper понимает, что необходимо выполнить массовую вставку и самостоятельно перепишет запрос,
       чтобы в базу данных вставились все записи.
4. Для выборки данных мы не используем Reader - мы можем получить перечисление объектов с помощью метода QueryAsync.  
   Так мы получим уже готовый для использования список объектов со всеми данными пользователей, который потом удобно
   будет использовать дальше в приложении.

## ORM на примере EntityFramework

Такое взаимоделйстве с базой данных имеет некоторые недостатки:

- Код остаётся всё равно сложный - необходимо вручную описывать много типовых SQL запросов на любые действия с базой
  данных;
- Легко допустить ошибку, которая будет обнаружена только после запуска приложения.

Можно ли в приложениях обойтись вообще без написания SQL запросов, полностью погрузившись в объектно-ориентированную
логику? То есть вместо написания кода :

```csharp
var users = new[]
{
    new UserCreateInfo("first_some_user"),
    new UserCreateInfo("second_some_user"),
    new UserCreateInfo("some_third_login")
};

var insertRecordsCommandText = $@"
    INSERT INTO user_info(login, created_on) 
    VALUES (@Login, now());";

await connection.ExecuteAsync(insertRecordsCommandText, new { firstUser, secondUser });
```

написать что то более лаконичное:

```csharp
var users = new[]
{
    new UserCreateInfo("first_some_user"),
    new UserCreateInfo("second_some_user"),
    new UserCreateInfo("some_third_login")
};

database.Add(users);
```

Да, такую возможность нам дают решения, называемые ORM (Object Relational Mapping - объектно реляционное связывание) -
они позволяют нам работать с базами данных путём манипулирования объектами в приложении.

ORM по сути скрывает от разработчика особенности базы данных, позволяя работать с:

- таблицами - как классами ;
- записями таблиц - как экземплярами классов;
- столбцами таблиц - как свойствами класса;
  а остальными сущностями БД (индексами, ограничениями и т.п) - через конфигурацию в коде приложения.

Решений класса ORM не так много, потому что они достаточно сложны в разработке.
В NET приложениях в основном вы можете встретить:

- Entity Framework
- NHibernate
- различные "микро-ORM", например Dapper, linq2db, Massive, PetaPoco и др.

Сейчас и в дальнейшем мы будем использовать Entity Framework как наиболее известную, продвинутую и имеющую наибольшее
количесто написанной документации.

Напишем приложение для такой же работы с пользователями с использованием Entity Framework, для этого:

- Создадим новое консольное приложение - через интерфейс или командой:
  ```shell
  dotnet new console --name Database.EntityFramework --framework net8.0
  ```
- Добавим зависимость от пакета Npgsql.EntityFrameworkCore.PostgreSQL 8.0.2 (или любой актуальной - этот пакет является
  адаптером EF к Postgresql и тянет за собой зависимости сразу от EntityFramework и Npgsql) - через интерфейс или
  командой:
  ```shell
  dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.2
  ```
- Установим инструменты командной строки для EF - они нам пригодятся чуть позже:
  ```shell
  dotnet tool install --global dotnet-ef
  ```
- Установим вспомогательный пакет для утилиты - через интерфейс или командой:
  ```shell
  dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.2
  ```
- Приступим к коду - опишем модель нашего пользователя в новом классе `UserInfo.cs`:
  ```csharp
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  
  namespace Database.EntityFramework;
  
  [Table("user_info", Schema = "public")]
  public class UserInfo
  {
      [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public long Id { get; set; }
  
      [Column("login", TypeName = "varchar(100)"), Required]
      public string Login { get; set; }
  
      [Column("created_on"), Required]
      public DateTime CreationDate { get; set; }
  }
  ```
- Опишем класс, через который будет происходить основное взаимодействие с базой данных - `ApplicationDbContext.cs`:
  ```csharp
  using Microsoft.EntityFrameworkCore;
  
  namespace Database.EntityFramework;
  
  /// <summary>
  /// Контекст БД в приложении.
  /// </summary>
  public class ApplicationDbContext : DbContext
  {
      private const string ConnectionString =
          "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";

      public DbSet<UserInfo> Users { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
          => modelBuilder.Entity<UserInfo>()
              .Property(u => u.CreationDate)
              .HasDefaultValueSql("now()");

      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
          => optionsBuilder.UseNpgsql(ConnectionString);
  }
  ```
- Мы описали код взаимодействия с БД и теперь мы можем описать код основного приложения - `Program.cs`
  ```csharp
  using Database.EntityFramework;
  using Microsoft.EntityFrameworkCore;
  
  // 1.
  await using var db = new ApplicationDbContext();
  
  // 2.
  var users = new[]
  {
      new UserInfo { Login = "first_some_user" },
      new UserInfo { Login = "second_some_user" },
      new UserInfo { Login = "some_third_login" },
  };
  await db.Users.AddRangeAsync(users);
  await db.SaveChangesAsync();
  
  // 3.
  var usersList = await db.Users.ToListAsync();
  foreach (var user in usersList)
  {
      Console.WriteLine($"Идентификатор {user.Id}, пользователь {user.Login}, дата создания - {user.CreationDate}");
  }
  ```
  В рамках данного класса мы выполняем создание (2) и просмотр (3) записей из БД полностью в объектной логике - внешне
  не зная об особенностях БД, её строении и не написав ни строчки SQL.

### Миграции схемы базы данных

Заметим, что в реализации через Dapper или непосредственно Npgsql мы самостоятельно описывали не только методы вставки и
получения данных из таблицы, но и само создание таблицы.

При использовании Entity Framework Core нам нет нужды самостоятельно прописывать создание таблицы - за нас это сделает
механизм миграции схемы базы данных (или просто - миграции).

Механизм миграций работает следующим образом:

* При изменении модели данных разработчик использует средства EF Core для автогенерации соответствующей миграции,
  необходимые для синхронизации схемы базы данных.   
  EF Core создаёт миграцию, определяя внесённые в модель изменения и генерируя соответствующий этим изменениям SQL код;
* Созданную миграцию можно применять либо вручную, либо автоматически при запуске приложения, либо любым другим
  способом.
* Все примененные миграции записываются в специальные таблицы БД - чтобы можно было понять, какие миграции уже были
  проведены.

Проведём миграцию схемы базы данных (должны быть установлены пакеты Microsoft.EntityFrameworkCore.Design и утилита
dotnet-ef):

- Выполним генерацию миграции (выполнять из директории проекта)  
  `dotnet ef migrations add InitialCreate`  
  После выполнения данной команды в вашем проекте появится директория `Migrations` с сгенерированными файлами миграции
- Проведём миграцию схемы базы данных  
  `dotnet ef database update`  
  В рамках этой команды в базе данных будут созданы соответствующие таблицы (в нашем случае - таблица `user_info` с
  соотв. столбцами).

### Заключение по ORM
Преимущества использования ORM после данной демонстрации должны быть вам очевидны:

- Вам не нужно писать кучу типового кода - за вас всё делает ORM
- Взаимодействие с БД происходит в естественном для приложения объектно-ориентированном подходе.
- _В теории_ вы сможете использовать приложение с разными типами БД (в реальности не всё так просто).

Однако, у ORM есть настолько же серьёзные недостатки:

- Производительность может быть несколько ниже. SQL-ом написать оптимальный код для специфичных случаев легче, добиться
  схожих результатов на ORM сложно;
- ORM, например EF - не самый простой инструмент, его нужно изучать отдельно;
- Абстрагирование от БД - иногда использование ORM разработчиками без понимания принципов работы БД приводит к крайне
  неоптимальным решениям в коде.

## Самостоятельная работа

1. В своём ASP.NET приложение добавьте взаимодействие с СУБД, используя Dapper или Entity Framework Core
   (предпочтительнее).
2. Попробуйте самостоятельно использовать механизм миграции схемы базы данных (при реализации с использованием EF).
3. Реализуйте методы управления сущностями – создание, удаление, изменение и получение (по идентификатору и список
   целиком).

Пример обоих видов решений (Dapper и EF) вы можете найти в директории `Database.AspNetCoreExample`.