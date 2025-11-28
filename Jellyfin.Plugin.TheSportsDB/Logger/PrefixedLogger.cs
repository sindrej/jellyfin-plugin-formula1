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
    /// <param name="logger">
    /// The underlying <see cref="ILogger{T}"/> instance to wrap.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public PrefixedLogger(ILogger<T> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to associate with the scope.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that ends the logical operation scope on dispose.
    /// </returns>
    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        return _logger.BeginScope(state)!; // Suppress nullable warning
    }

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">Level to check.</param>
    /// <returns>
    /// <c>true</c> if enabled; otherwise, <c>false</c>.
    /// </returns>
    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }

    /// <summary>
    /// Writes a log entry with the specified level, event ID, state, and exception.
    /// The message is prefixed with a fixed string for identification.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">The event ID associated with the log.</param>
    /// <param name="state">The entry to be written. Can also an object.</param>
    /// <param name="exception">The exception related to this entry (if any).</param>
    /// <param name="formatter">
    /// Function to create a <see cref="string"/> message of the <paramref name="state"/> and <paramref name="exception"/>.
    /// </param>
    void ILogger.Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        _logger.Log(
            logLevel,
            eventId,
            state,
            exception,
            (s, e) => $"{Prefix} {formatter(s, e)}");
    }
}
