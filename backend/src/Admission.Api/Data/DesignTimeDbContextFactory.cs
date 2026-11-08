using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Admission.Api.Data;

/// <summary>
/// Used only by `dotnet ef` to build the model. It never connects to a database,
/// so the connection string below is a placeholder.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_only;Username=x;Password=x")
            .Options;
        return new AppDbContext(options);
    }
}
