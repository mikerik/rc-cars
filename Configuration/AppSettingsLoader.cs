using Microsoft.Extensions.Configuration;

namespace RcCars.Configuration;

/// <summary>
/// Loads <see cref="AppSettings"/>. Later sources override earlier ones:
/// <list type="number">
/// <item>appsettings.json</item>
/// <item>appsettings.{Environment}.json (optional), where the environment comes from DOTNET_ENVIRONMENT</item>
/// <item>Environment variables prefixed with RCCARS_, e.g. RCCARS_Commands__Forward__DistanceMeters=2</item>
/// <item>Command line arguments, e.g. --Commands:Forward:DistanceMeters=2</item>
/// </list>
/// </summary>
public static class AppSettingsLoader
{
    public const string BaseFileName = "appsettings.json";
    public const string EnvironmentVariableName = "DOTNET_ENVIRONMENT";
    public const string DefaultEnvironmentName = "Production";
    public const string EnvironmentVariablePrefix = "RCCARS_";

    /// <summary>The current environment name, e.g. "Development". Defaults to <see cref="DefaultEnvironmentName"/>.</summary>
    public static string EnvironmentName
    {
        get
        {
            var name = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            return string.IsNullOrWhiteSpace(name) ? DefaultEnvironmentName : name.Trim();
        }
    }

    /// <summary>Loads and validates the settings for the current environment from the application folder.</summary>
    /// <exception cref="InvalidConfigurationException">The configuration cannot be read or is invalid.</exception>
    public static AppSettings Load(string[] args) => Load(AppContext.BaseDirectory, EnvironmentName, args);

    /// <exception cref="InvalidConfigurationException">The configuration cannot be read or is invalid.</exception>
    public static AppSettings Load(string basePath, string environmentName, string[] args)
    {
        IConfiguration configuration;
        try
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(BaseFileName, optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables(EnvironmentVariablePrefix)
                .AddCommandLine(args)
                .Build();
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or FormatException)
        {
            throw new InvalidConfigurationException(ex.Message, ex);
        }

        return Bind(configuration);
    }

    /// <summary>Binds and validates the settings. Unknown keys are rejected to catch typos.</summary>
    /// <exception cref="InvalidConfigurationException">The configuration contains unknown keys or invalid values.</exception>
    public static AppSettings Bind(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        AppSettings settings;
        try
        {
            settings = configuration.Get<AppSettings>(options => options.ErrorOnUnknownConfiguration = true) ?? new AppSettings();
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidConfigurationException(ex.Message, ex);
        }

        var errors = settings.Validate();
        if (errors.Count > 0)
        {
            throw new InvalidConfigurationException(string.Join(Environment.NewLine, errors));
        }

        return settings;
    }
}
