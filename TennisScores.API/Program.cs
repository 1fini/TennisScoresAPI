using Microsoft.EntityFrameworkCore;
using TennisScores.API.Services;
using TennisScores.Domain;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure;
using TennisScores.Infrastructure.Data;
using TennisScores.Infrastructure.Repositories;
using TennisScoresAPI.Hubs;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSignalR();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader());
        });

        builder.Services.AddDbContext<TennisDbContext>(options =>
                options.UseNpgsql($"Host={builder.Configuration["DB_HOST"]};" +
                    $"Port={builder.Configuration["DB_PORT"]};" +
                    $"Database={builder.Configuration["DB_NAME"]};" +
                    $"Username={builder.Configuration["DB_USER"]};" +
                    $"Password={builder.Configuration["DB_PASSWORD"]};"));

        builder.Services.AddScoped<IMatchRepository, MatchRepository>();
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IMatchFormatRepository, MatchFormatRepository>();
        builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
        builder.Services.AddScoped<ISetRepository, SetRepository>();
        builder.Services.AddScoped<IGameRepository, GameRepository>();
        builder.Services.AddScoped<IPointRepository, PointRepository>();
        builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();

        builder.Services.AddScoped<IMatchService, MatchService>();
        builder.Services.AddScoped<IMatchFormatService, MatchFormatService>();
        builder.Services.AddScoped<IPlayerService, PlayerService>();
        builder.Services.AddScoped<ITournamentService, TournamentService>();
        builder.Services.AddScoped<ILiveScoreService, LiveScoreService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<LiveScoreService>();



        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.MapHub<ScoreHub>("/scoreHub");

        app.Run();
    }
}