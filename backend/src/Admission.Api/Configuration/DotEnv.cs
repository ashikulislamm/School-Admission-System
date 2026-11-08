namespace Admission.Api.Configuration;

/// <summary>
/// Development-only convenience: loads KEY=VALUE lines from .env files into the process
/// environment so `dotnet run` behaves like Docker. Real environment variables always win.
/// </summary>
public static class DotEnv
{
    private const int MaxLevelsUp = 4;

    public static void LoadIfDevelopment()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (!string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var level = 0; level <= MaxLevelsUp && dir is not null; level++, dir = dir.Parent)
        {
            var file = Path.Combine(dir.FullName, ".env");
            if (File.Exists(file))
            {
                Load(file);
            }
        }
    }

    public static void Load(string path)
    {
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var eq = line.IndexOf('=');
            if (eq <= 0)
            {
                continue;
            }

            var key = line[..eq].Trim();
            var value = line[(eq + 1)..].Trim().Trim('"');

            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
