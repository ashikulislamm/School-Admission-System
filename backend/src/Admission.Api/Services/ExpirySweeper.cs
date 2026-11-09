using Admission.Api.Configuration;
using Admission.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Admission.Api.Services;

/// <summary>
/// Hygiene only: persists EXPIRED for lapsed holds. Correctness never depends on it
/// because every read and count evaluates expiry lazily. Safe with many processes (SKIP LOCKED).
/// </summary>
public sealed class ExpirySweeper(IServiceScopeFactory scopes, AppSettings settings, ILogger<ExpirySweeper> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!settings.SweeperEnabled)
        {
            return;
        }

        using var timer = new PeriodicTimer(Interval);
        while (await WaitAsync(timer, stoppingToken))
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.ExecuteSqlAsync($"""
                    UPDATE holds SET status = 'EXPIRED'
                    WHERE id IN (
                        SELECT id FROM holds
                        WHERE status = 'HELD' AND expires_at <= clock_timestamp()
                        ORDER BY id
                        FOR UPDATE SKIP LOCKED)
                    """, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Expiry sweep failed; will retry.");
            }
        }
    }

    private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
