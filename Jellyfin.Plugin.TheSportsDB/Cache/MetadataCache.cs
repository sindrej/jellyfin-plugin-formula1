using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TheSportsDB.Cache;

/// <summary>
/// Simple file-based cache for API responses.
/// </summary>
public class MetadataCache
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private readonly IApplicationPaths _applicationPaths;
    private readonly ILogger _logger;
    private readonly string _cacheDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataCache"/> class.
    /// </summary>
    /// <param name="applicationPaths">The application paths.</param>
    /// <param name="logger">The logger.</param>
    public MetadataCache(IApplicationPaths applicationPaths, ILogger logger)
    {
        _applicationPaths = applicationPaths;
        _logger = logger;
        _cacheDirectory = Path.Combine(_applicationPaths.CachePath, "thesportsdb");

        // Ensure cache directory exists
        Directory.CreateDirectory(_cacheDirectory);
    }

    /// <summary>
    /// Gets a cached item.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached item if found and not expired, null otherwise.</returns>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            var filePath = GetCacheFilePath(key);
            if (!File.Exists(filePath))
            {
                return null;
            }

            var fileInfo = new FileInfo(filePath);
            var cacheDuration = TimeSpan.FromDays(Plugin.Instance?.Configuration?.CacheDurationDays ?? 7);

            // Check if cache is expired
            if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc > cacheDuration)
            {
                _logger.LogDebug("Cache expired for key: {Key}", key);
                File.Delete(filePath);
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            var item = JsonSerializer.Deserialize<T>(json);

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading cache for key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Sets a cached item.
    /// </summary>
    /// <typeparam name="T">The type of the item to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            var filePath = GetCacheFilePath(key);
            var json = JsonSerializer.Serialize(value, JsonOptions);

            await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Cache set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing cache for key: {Key}", key);
        }
    }

    /// <summary>
    /// Clears all cached items.
    /// </summary>
    public void Clear()
    {
        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, true);
                Directory.CreateDirectory(_cacheDirectory);
                _logger.LogInformation("Cache cleared");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }

    /// <summary>
    /// Gets the file path for a cache key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The file path.</returns>
    private string GetCacheFilePath(string key)
    {
        var hash = ComputeHash(key);
        return Path.Combine(_cacheDirectory, $"{hash}.json");
    }

    /// <summary>
    /// Computes a hash for the given key.
    /// </summary>
    /// <param name="key">The key to hash.</param>
    /// <returns>The hex-encoded hash.</returns>
    private string ComputeHash(string key)
    {
        var bytes = Encoding.UTF8.GetBytes(key);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
