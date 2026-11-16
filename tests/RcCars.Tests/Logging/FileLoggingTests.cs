using Microsoft.Extensions.Logging;
using RcCars.Configuration;
using RcCars.Logging;

namespace RcCars.Tests.Logging;

public sealed class FileLoggingTests : IDisposable
{
    private readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory();

    public void Dispose() => _folder.Delete(recursive: true);

    private string LogPath => Path.Combine(_folder.FullName, "logs", "test.log");

    private ILoggerFactory CreateFactory(Action<LoggingSettings>? configure = null)
    {
        var settings = new LoggingSettings { File = { Enabled = true, Path = "logs/test.log" } };
        configure?.Invoke(settings);
        return LoggingSetup.CreateLoggerFactory(settings, _folder.FullName);
    }

    [Fact]
    public void Provider_WritesFormattedLineWithScopeAndException()
    {
        var time = new FixedTimeProvider(new DateTimeOffset(2026, 11, 16, 15, 20, 1, 123, TimeSpan.Zero));
        using (var factory = new LoggerFactory())
        {
            factory.AddProvider(new FileLoggerProvider(LogPath, time));
            var logger = factory.CreateLogger("Test.Category");

            using (logger.BeginScope("Simulation #{SimulationNumber}", 2))
            {
                logger.LogWarning(new EventId(42), "Something odd: {Value}", 7);
            }

            logger.LogError(new InvalidOperationException("boom"), "Failed");
        }

        var lines = File.ReadAllLines(LogPath);
        Assert.Equal("2026-11-16 15:20:01.123 +00:00 [WRN] Test.Category[42] => Simulation #2: Something odd: 7", lines[0]);
        Assert.Equal("2026-11-16 15:20:01.123 +00:00 [ERR] Test.Category[0]: Failed", lines[1]);
        Assert.Contains("System.InvalidOperationException: boom", lines[2]);
    }

    [Fact]
    public void Provider_AppendsToExistingFile()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
        File.WriteAllText(LogPath, "existing" + Environment.NewLine);

        using (var provider = new FileLoggerProvider(LogPath))
        {
            provider.CreateLogger("Test").LogInformation("appended");
        }

        var lines = File.ReadAllLines(LogPath);
        Assert.Equal("existing", lines[0]);
        Assert.EndsWith("appended", lines[1]);
    }

    [Fact]
    public void Provider_AfterDispose_IgnoresEntries()
    {
        var provider = new FileLoggerProvider(LogPath);
        var logger = provider.CreateLogger("Test");
        provider.Dispose();

        logger.LogInformation("ignored");

        Assert.Equal(string.Empty, File.ReadAllText(LogPath));
    }

    [Fact]
    public void CreateLoggerFactory_FileEnabled_WritesRelativeToBaseDirectory()
    {
        using (var factory = CreateFactory())
        {
            factory.CreateLogger("Test").LogInformation("hello");
        }

        Assert.Contains("[INF] Test[0]: hello", File.ReadAllText(LogPath));
    }

    [Fact]
    public void CreateLoggerFactory_AppliesDefaultAndCategoryLevels()
    {
        using (var factory = CreateFactory(settings =>
               {
                   settings.LogLevel["Default"] = LogLevel.Warning;
                   settings.LogLevel["RcCars.Simulation"] = LogLevel.Trace;
               }))
        {
            factory.CreateLogger("RcCars.UI.SimulatorConsoleApp").LogInformation("hidden info");
            factory.CreateLogger("RcCars.UI.SimulatorConsoleApp").LogWarning("visible warning");
            factory.CreateLogger("RcCars.Simulation.Simulator").LogTrace("visible trace");
        }

        var log = File.ReadAllText(LogPath);
        Assert.DoesNotContain("hidden info", log);
        Assert.Contains("visible warning", log);
        Assert.Contains("visible trace", log);
    }

    [Fact]
    public void CreateLoggerFactory_FileDisabled_DoesNotCreateFile()
    {
        using (var factory = CreateFactory(settings => settings.File.Enabled = false))
        {
            factory.CreateLogger("Test").LogError("not written");
        }

        Assert.False(File.Exists(LogPath));
    }

    [Fact]
    public void CreateLoggerFactory_FileCannotBeOpened_DoesNotThrow()
    {
        // A folder with the same name as the log file makes opening the file fail.
        Directory.CreateDirectory(LogPath);

        using var factory = CreateFactory();

        factory.CreateLogger("Test").LogInformation("still works");
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
