using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceUsers.Data;
using ServiceUsers.Models;
using ServiceUsers.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================
// DATABASE CONFIG
// =====================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=users.db"));

// =====================
// IDENTITY CONFIG
// =====================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// =====================
// JWT CONFIG
// =====================
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("JWT Key is missing in configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // можно true, если HTTPS
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// =====================
// DEPENDENCY INJECTION
// =====================
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5076); // HTTP
    // options.ListenAnyIP(7117, listenOptions => listenOptions.UseHttps()); // HTTPS если нужно
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ServiceUsers API",
        Version = "v1",
        Description = "API for ServiceUsers"
    });

    // JWT support in Swagger
    c.AddSecurityDefinition("bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "bearer" } },
            new string[] {}
        }
    });
});

// =====================
// BUILD APP
// =====================
var app = builder.Build();

// =====================
// APPLY MIGRATIONS
// =====================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// =====================
// MIDDLEWARE PIPELINE
// =====================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}


if (app.Environment.IsDevelopment() || true) // можно включить в тест/prod по надобности
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceUsers v1");
        c.RoutePrefix = "docs"; // -> доступ по /docs
    });
}

//app.UseHttpsRedirection();

// ВАЖНО: порядок имеет значение
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
