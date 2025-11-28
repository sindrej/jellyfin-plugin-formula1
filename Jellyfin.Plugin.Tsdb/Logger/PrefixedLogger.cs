namespace Jellyfin.Plugin.Tsdb.Logger;

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

/// <summary>
/// A logger that prefixes all log messages with [TheSportsDB].
/// </summary>
/// <typeparam name="T">The type whose name is used for the logger category.</typeparam>
[SuppressMessage("LoggingGenerator", "CA2254", Justification = "Template variation is intentional for prefix")]
public sealed class PrefixedLogger<T>
{
    private const string Prefix = "[TheSportsDB]";
    private readonly ILogger<T> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixedLogger{T}"/> class.
    /// </summary>
    /// <param name="logger">The underlying logger.</param>
    public PrefixedLogger(ILogger<T> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    public void LogTrace(string message, params object?[] args)
    {
        _logger.LogTrace($"{Prefix} {message}", args);
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    public void LogDebug(string message, params object?[] args)
    {
        _logger.LogDebug($"{Prefix} {message}", args);
    }

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    public void LogInformation(string message, params object?[] args)
    {
        _logger.LogInformation($"{Prefix} {message}", args);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    public void LogWarning(string message, params object?[] args)
    {
        _logger.LogWarning($"{Prefix} {message}", args);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    public void LogError(string message, params object?[] args)
    {
        _logger.LogError($"{Prefix} {message}", args);
    }

    /// <summary>
    /// Logs an error message with exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    public void LogError(Exception exception, string message, params object?[] args)
    {
        _logger.LogError(exception, $"{Prefix} {message}", args);
    }

    /// <summary>
    /// Logs a critical message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    public void LogCritical(string message, params object?[] args)
    {
        _logger.LogCritical($"{Prefix} {message}", args);
    }

    /// <summary>
    /// Logs a critical message with exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="message">The message template.</param>
    /// <param name="args">The message arguments.</param>
    public void LogCritical(Exception exception, string message, params object?[] args)
    {
        _logger.LogCritical(exception, $"{Prefix} {message}", args);
    }
}
