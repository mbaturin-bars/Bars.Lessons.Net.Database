using Database.AspNetCoreExample.Services;

var builder = WebApplication.CreateBuilder(args);

// Генерация документации Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация источника для соединений с БД.
var connectionString = builder.Configuration.GetConnectionString("default")!;
builder.Services.AddNpgsqlDataSource(connectionString);
builder.Services.AddSingleton<IUserService, DapperUserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();