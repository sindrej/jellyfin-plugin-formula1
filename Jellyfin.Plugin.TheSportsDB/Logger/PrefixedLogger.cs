using System;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TheSportsDB.Logger;

/// <summary>
/// A logger wrapper that prefixes all log messages with a fixed string.
/// This is useful for distinguishing logs from a specific component or plugin.
/// </summary>
/// <typeparam name="T">
/// The type whose name is used for the logger category.
/// </typeparam>
public sealed class PrefixedLogger<T> : ILogger<T>
{
    private const string Prefix = "[TheSportsDB]";
    private readonly ILogger<T> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixedLogger{T}"/> class.
    /// </summary>
    /// <param name="logger">The underlying <see cref="ILogger{T}"/> instance to wrap.</param>
    public PrefixedLogger(ILogger<T> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        return _logger.BeginScope(state)!;
    }

    /// <inheritdoc />
    bool ILogger.IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    /// <inheritdoc />
    void ILogger.Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        // Apply prefix to the final formatted message
        var originalMessage = formatter(state, exception);
        _logger.Log(logLevel, eventId, state, exception, (s, e) => $"{Prefix} {originalMessage}");
    }

    /// <summary>
    /// Logs a trace message with prefix.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    public void LogTrace(string message, params object[] args) =>
        _logger.LogTrace("{Prefix} {Message}", Prefix, message);

    /// <summary>
    /// Logs a debug message with prefix.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    public void LogDebug(string message, params object[] args) =>
        _logger.LogDebug("{Prefix} {Message}", Prefix, message);

    /// <summary>
    /// Logs an informational message with prefix.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    public void LogInformation(string message, params object[] args) =>
        _logger.LogInformation("{Prefix} {Message}", Prefix, message);

    /// <summary>
    /// Logs a warning message with prefix.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    public void LogWarning(string message, params object[] args) =>
        _logger.LogWarning("{Prefix} {Message}", Prefix, message);

    /// <summary>
    /// Logs an error message with prefix.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    public void LogError(string message, params object[] args) =>
        _logger.LogError("{Prefix} {Message}", Prefix, message);

    /// <summary>
    /// Logs an error message with exception and prefix.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    public void LogError(Exception exception, string message, params object[] args) =>
        _logger.LogError(exception, "{Prefix} {Message}", Prefix, message);

    /// <summary>
    /// Logs a critical message with prefix.
    /// </summary>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    public void LogCritical(string message, params object[] args) =>
        _logger.LogCritical("{Prefix} {Message}", Prefix, message);

    /// <summary>
    /// Logs a critical message with exception and prefix.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message template.</param>
    /// <param name="args">Optional arguments for message formatting.</param>
    public void LogCritical(Exception exception, string message, params object[] args) =>
        _logger.LogCritical(exception, "{Prefix} {Message}", Prefix, message);
}
