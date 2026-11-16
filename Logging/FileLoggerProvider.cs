using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace RcCars.Logging;

/// <summary>
/// Appends log entries to a text file, one line per entry followed by any exception details.
/// The file is shared for reading so it can be followed while the simulator runs.
/// </summary>
[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly Lock _lock = new();
    private readonly StreamWriter _writer;
    private readonly TimeProvider _timeProvider;
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();
    private bool _disposed;

    /// <exception cref="IOException">The file cannot be created or opened.</exception>
    /// <exception cref="UnauthorizedAccessException">Access to the file or folder is denied.</exception>
    public FileLoggerProvider(string path, TimeProvider? timeProvider = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        FilePath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var stream = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        _writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) { AutoFlush = true };
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>The full path of the log file.</summary>
    public string FilePath { get; }

    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) =>
        _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _writer.Dispose();
        }
    }

    private void Write(string categoryName, LogLevel logLevel, EventId eventId, string message, Exception? exception)
    {
        var entry = new StringBuilder()
            .Append(_timeProvider.GetLocalNow().ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture))
            .Append(" [").Append(GetLevelText(logLevel)).Append("] ")
            .Append(categoryName).Append('[').Append(eventId.Id).Append(']');

        _scopeProvider.ForEachScope((scope, builder) => builder.Append(" => ").Append(scope), entry);

        entry.Append(": ").Append(message);
        if (exception is not null)
        {
            entry.AppendLine().Append(exception);
        }

        lock (_lock)
        {
            if (!_disposed)
            {
                _writer.WriteLine(entry.ToString());
            }
        }
    }

    private static string GetLevelText(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        LogLevel.Critical => "CRT",
        _ => logLevel.ToString(),
    };

    private sealed class FileLogger(FileLoggerProvider provider, string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => provider._scopeProvider.Push(state);

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception is null)
            {
                return;
            }

            provider.Write(categoryName, logLevel, eventId, message, exception);
        }
    }
}
