using Microsoft.EntityFrameworkCore;

namespace Admission.Api.Data;

public static class DatabaseInitializer
{
    private const int MaxAttempts = 30;
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Applies pending migrations, retrying while the database is still starting up.
    /// EF Core takes a database-level lock while migrating, so two processes starting
    /// together do not apply the same migration twice.
    /// </summary>
    public static async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Database is up to date.");
                return;
            }
            catch (Exception ex) when (attempt < MaxAttempts && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "Database not ready (attempt {Attempt}/{Max}): {Message}",
                    attempt,
                    MaxAttempts,
                    ex.Message);
                await Task.Delay(Delay, cancellationToken);
            }
        }
    }
}
