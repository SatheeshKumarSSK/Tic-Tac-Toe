using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Data;
using TicTacToe.Api.Middlewares;
using TicTacToe.Api.Models.Entities;
using TicTacToe.Api.Repositories;
using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials();
    });
});

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

//Services
builder.Services.AddScoped<IGameService, GameService>();

//Repositories
builder.Services.AddScoped<IGameRepository, GameRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    context.Database.EnsureCreated();

    if (!context.Scoreboards.Any())
    {
        context.Scoreboards.Add(new Scoreboard()
        {
            XWins = 0,
            OWins = 0,
            Draws = 0,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        });
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.Run();
