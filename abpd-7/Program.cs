using apbd_7.Data;
using apbd_7.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Konfiguracja kontekstu bazy danych
// ConnectionString jest pobierany z appsettings.json, oczywiście należy go tam też ustawić
builder.Services.AddDbContext<ApbdContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// Wstrzykiwanie zależności
// https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
builder.Services.AddScoped<IDBService, DBService>();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();