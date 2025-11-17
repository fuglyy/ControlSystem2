var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 //   app.MapOpenApi();
}
app.UseCors();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Request-ID"))
    {
        context.Request.Headers["X-Request-ID"] = Guid.NewGuid().ToString();
    }

    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Request-ID"] = context.Request.Headers["X-Request-ID"];
        return Task.CompletedTask;
    });

    await next();
});




app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
