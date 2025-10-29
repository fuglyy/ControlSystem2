using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ServiceUsers.Data;
using ServiceUsers.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    // Получаем строку подключения из appsettings.json, если её нет, используем users.db
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=users.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

var app = builder.Build();

// Применение миграций при запуске
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate(); 
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   // Здесь останется пусто
}

app.UseHttpsRedirection();

// app.MapGet("/status", ...) - если вы его добавляли, он должен быть тут.

app.MapControllers(); // Добавляет поддержку маршрутов из контроллеров

app.Run();
