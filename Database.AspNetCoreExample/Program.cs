using System.Reflection;
using System.Xml.XPath;
using Database.AspNetCoreExample.Services;
using Database.AspNetCoreExample.Services.Dapper;
using Database.AspNetCoreExample.Services.EntityFramework;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Генерация документации Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(IncludeSwaggerCommentsFromXml);

var connectionString = builder.Configuration.GetConnectionString("default")!;

// Регистрация сервиса для работы с БД.
// Если в конфигурации задан ключ RealizationType:Dapper - подключаем реализацию с Dapper.
// Если RealizationType:EntityFramework - подключаем реализацию с EntityFramework.
switch (builder.Configuration["RealizationType"])
{
    case "Dapper":
        builder.Services.AddNpgsqlDataSource(connectionString);
        builder.Services.AddSingleton<IUserService, DapperUserService>();
        Console.WriteLine("Используется реализация Dapper");
        break;
    case "EntityFramework":
        builder.Services.AddScoped<UserDbContext>(_ => new UserDbContext(connectionString));
        builder.Services.AddScoped<IUserService, EntityFrameworkUserService>();
        Console.WriteLine("Используется реализация EntityFramework");
        break;
    default:
        throw new Exception("Некорректная конфигурация - ключ RealizationType " +
                            "должен иметь значение Dapper или EntityFramework!");
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();

// Включить описание эндпоинтов для Swagger из XML документации (из summary).
void IncludeSwaggerCommentsFromXml(SwaggerGenOptions options)
{
    options.IncludeXmlComments(() =>
    {
        var assembly = Assembly.GetExecutingAssembly();

        if (assembly?.GetManifestResourceNames().FirstOrDefault(x => x.Contains("swagger.xml"))
            is not { } swaggerResourceName)
        {
            throw new FileNotFoundException("Не найден файл с XML документацией для Swagger");
        }

        using var resource = assembly.GetManifestResourceStream(swaggerResourceName);
        return resource is null
            ? throw new FileNotFoundException("Не найден файл с XML документацией для Swagger")
            : new XPathDocument(resource);
    }, true);
}