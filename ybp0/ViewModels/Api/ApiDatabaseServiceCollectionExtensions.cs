using DataBase;
using DataBase.Interfaces;
using DataBase.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ViewModels.Api;

public static class ApiDatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddYbp0ApiDatabase(this IServiceCollection services, string webServicesContentRootPath)
    {
        string databasePath = GetDatabaseProjectSqlitePath(webServicesContentRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath};Pooling=False"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITrainerRepository, TrainerRepository>();
        services.AddScoped<ITraineeRepository, TraineeRepository>();
        services.AddScoped<IMuscleRepository, MuscleRepository>();
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
        services.AddScoped<IWeekPlanRepository, WeekPlanRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        return services;
    }

    public static void EnsureYbp0ApiDatabaseCreated(this IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ILogger<AppDbContext> logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
        db.Database.EnsureCreated();
        logger.LogInformation("YBP0 API database is ready at {DatabasePath}", db.Database.GetDbConnection().DataSource);
    }

    private static string GetDatabaseProjectSqlitePath(string webServicesContentRootPath)
    {
        string solutionRoot = Path.GetFullPath(Path.Combine(webServicesContentRootPath, ".."));
        return Path.Combine(solutionRoot, "DataBase", "DataBase", "ybp0.db");
    }
}
