using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RcCars.Configuration;

namespace RcCars.Tests.Configuration;

public class AppSettingsLoaderTests
{
    private static AppSettings LoadShipped(string environmentName, params string[] args) =>
        AppSettingsLoader.Load(AppContext.BaseDirectory, environmentName, args);

    private static AppSettings Bind(params (string Key, string? Value)[] values) =>
        AppSettingsLoader.Bind(new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build());

    private static InvalidConfigurationException BindInvalid(params (string Key, string? Value)[] values) =>
        Assert.Throws<InvalidConfigurationException>(() => Bind(values));

    /// <summary>Runs <paramref name="test"/> with a temporary folder containing the given files.</summary>
    private static void WithConfigFiles(Action<string> test, params (string Name, string Content)[] files)
    {
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            foreach (var (name, content) in files)
            {
                File.WriteAllText(Path.Combine(folder.FullName, name), content);
            }

            test(folder.FullName);
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    [Fact]
    public void Load_ShippedProductionSettings_MatchCodeDefaults()
    {
        Assert.Equivalent(new AppSettings(), LoadShipped("Production"), strict: true);
    }

    [Fact]
    public void Load_ShippedDevelopmentSettings_AreValid()
    {
        Assert.NotNull(LoadShipped("Development"));
    }

    [Fact]
    public void Load_EnvironmentFile_OverridesOnlyItsValues()
    {
        WithConfigFiles(
            folder =>
            {
                var settings = AppSettingsLoader.Load(folder, "Staging", []);

                Assert.Equal(2, settings.Commands.Forward.DistanceMeters);
                Assert.Equal(1, settings.Commands.Back.DistanceMeters);
                Assert.Equal('F', settings.Commands.Forward.Code);
            },
            (AppSettingsLoader.BaseFileName, """{ "Commands": { "Forward": { "Code": "F" } } }"""),
            ("appsettings.Staging.json", """{ "Commands": { "Forward": { "DistanceMeters": 2 } } }"""));
    }

    [Fact]
    public void Load_UnknownEnvironment_UsesBaseSettings()
    {
        var settings = LoadShipped("NoSuchEnvironment");

        Assert.Equal(1, settings.Commands.Forward.DistanceMeters);
    }

    [Fact]
    public void Load_CommandLineArguments_OverrideFiles()
    {
        var settings = LoadShipped("Production", "--Commands:Forward:DistanceMeters=3", "--Commands:Forward:Code=U");

        Assert.Equal(3, settings.Commands.Forward.DistanceMeters);
        Assert.Equal('U', settings.Commands.Forward.Code);
    }

    [Fact]
    public void Load_PrefixedEnvironmentVariable_OverridesFilesButNotCommandLine()
    {
        const string name = AppSettingsLoader.EnvironmentVariablePrefix + "Commands__Back__DistanceMeters";
        Environment.SetEnvironmentVariable(name, "4");
        try
        {
            Assert.Equal(4, LoadShipped("Production").Commands.Back.DistanceMeters);
            Assert.Equal(5, LoadShipped("Production", "--Commands:Back:DistanceMeters=5").Commands.Back.DistanceMeters);
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Fact]
    public void Load_MissingBaseFile_Throws()
    {
        WithConfigFiles(folder =>
        {
            var ex = Assert.Throws<InvalidConfigurationException>(() => AppSettingsLoader.Load(folder, "Production", []));

            Assert.Contains(AppSettingsLoader.BaseFileName, ex.Message);
        });
    }

    [Fact]
    public void Load_MalformedJson_Throws()
    {
        WithConfigFiles(
            folder => Assert.Throws<InvalidConfigurationException>(() => AppSettingsLoader.Load(folder, "Production", [])),
            (AppSettingsLoader.BaseFileName, "{ \"Commands\": "));
    }

    [Fact]
    public void Bind_Empty_UsesDefaults()
    {
        Assert.Equivalent(new AppSettings(), Bind(), strict: true);
    }

    [Fact]
    public void Bind_UnknownKey_Throws()
    {
        var ex = BindInvalid(("Commands:Forward:Distance", "2"));

        Assert.Contains("Distance", ex.Message);
    }

    [Theory]
    [InlineData("Commands:Forward:DistanceMeters", "1.5")]
    [InlineData("Commands:Back:DistanceMeters", "far")]
    [InlineData("Commands:Forward:Code", "FF")]
    public void Bind_ValueOfWrongType_Throws(string key, string value)
    {
        BindInvalid((key, value));
    }

    [Theory]
    [InlineData("Commands:Forward:DistanceMeters", "-1", "Commands:Forward:DistanceMeters must be at least 1")]
    [InlineData("Commands:Back:DistanceMeters", "0", "Commands:Back:DistanceMeters must be at least 1")]
    [InlineData("Commands:TurnLeft:Code", " ", "Commands:TurnLeft:Code must be a single visible character")]
    [InlineData("Commands:TurnRight:Description", "", "Commands:TurnRight:Description must not be empty")]
    public void Bind_InvalidValue_ThrowsDescriptiveError(string key, string value, string expectedMessagePart)
    {
        var ex = BindInvalid((key, value));

        Assert.Contains(expectedMessagePart, ex.Message);
    }

    [Fact]
    public void Bind_DuplicateCommandCodesIgnoringCase_Throws()
    {
        var ex = BindInvalid(("Commands:Back:Code", "f"));

        Assert.Contains("Forward, Back all use the code 'F'", ex.Message);
    }

    [Fact]
    public void Bind_SeveralErrors_ReportsAll()
    {
        var ex = BindInvalid(("Commands:Forward:DistanceMeters", "0"), ("Commands:Back:DistanceMeters", "0"));

        Assert.Contains("Forward:DistanceMeters", ex.Message);
        Assert.Contains("Back:DistanceMeters", ex.Message);
    }

    [Fact]
    public void Load_ShippedDevelopmentSettings_EnableVerboseFileLogging()
    {
        var settings = LoadShipped("Development");

        Assert.True(settings.Logging.File.Enabled);
        Assert.Equal(LogLevel.Debug, settings.Logging.LogLevel["Default"]);
        Assert.Equal(LogLevel.Trace, settings.Logging.LogLevel["RcCars.Simulation"]);
    }

    [Fact]
    public void Bind_LoggingSettings_AddsCategoryLevelsToDefault()
    {
        var settings = Bind(
            ("Logging:LogLevel:RcCars.UI", "Warning"),
            ("Logging:File:Enabled", "true"),
            ("Logging:File:Path", "custom.log"));

        Assert.Equal(LogLevel.Information, settings.Logging.LogLevel["Default"]);
        Assert.Equal(LogLevel.Warning, settings.Logging.LogLevel["RcCars.UI"]);
        Assert.True(settings.Logging.File.Enabled);
        Assert.Equal("custom.log", settings.Logging.File.Path);
    }

    [Theory]
    [InlineData("Logging:LogLevel:Default", "Verbose")]
    [InlineData("Logging:File:Enabled", "sometimes")]
    [InlineData("Logging:File:Folder", "logs")]
    public void Bind_InvalidLoggingValue_Throws(string key, string value)
    {
        BindInvalid((key, value));
    }

    [Fact]
    public void Bind_UndefinedLogLevelNumber_ThrowsDescriptiveError()
    {
        var ex = BindInvalid(("Logging:LogLevel:Default", "42"));

        Assert.Contains("Logging:LogLevel:Default must be one of Trace, Debug", ex.Message);
    }

    [Fact]
    public void Bind_FileLoggingEnabledWithoutPath_ThrowsDescriptiveError()
    {
        var ex = BindInvalid(("Logging:File:Enabled", "true"), ("Logging:File:Path", ""));

        Assert.Contains("Logging:File:Path must not be empty", ex.Message);
    }
}
