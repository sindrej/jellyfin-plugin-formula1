using System;
using System.Collections;
using System.Collections.Generic;
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
        if (!_logger.IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);

        // Create a prefixed state wrapper that implements IReadOnlyList for structured logging
        var prefixedState = new PrefixedState<TState>(state, formatter, Prefix);

        _logger.Log(
            logLevel,
            eventId,
            prefixedState,
            exception,
            PrefixedState<TState>.Format);
    }

    private sealed class PrefixedState<TState> : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly TState _state;
        private readonly Func<TState, Exception?, string> _formatter;
        private readonly string _prefix;
        private readonly IReadOnlyList<KeyValuePair<string, object?>>? _stateList;

        public PrefixedState(TState state, Func<TState, Exception?, string> formatter, string prefix)
        {
            _state = state;
            _formatter = formatter;
            _prefix = prefix;
            _stateList = state as IReadOnlyList<KeyValuePair<string, object?>>;
        }

        // Implement IReadOnlyList to support structured logging
        public int Count => _stateList?.Count ?? 0;

        public KeyValuePair<string, object?> this[int index] => _stateList?[index] ?? default;

        public static string Format(PrefixedState<TState> state, Exception? exception)
        {
            var formatted = state._formatter(state._state, exception);
            return $"{state._prefix} {formatted}";
        }

        public override string ToString() => Format(this, null);

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return _stateList?.GetEnumerator() ??
                   ((IEnumerable<KeyValuePair<string, object?>>)Array.Empty<KeyValuePair<string, object?>>()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
