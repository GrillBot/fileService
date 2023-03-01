using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Sas;
using Microsoft.Extensions.Caching.Memory;

namespace FileService.Cache;

public class StorageCacheManager
{
    private IMemoryCache Cache { get; }

    private static readonly object Locker = new();

    private static DateTimeOffset FileExpirationAt
        => DateTimeOffset.Now.AddDays(1);

    public StorageCacheManager(IMemoryCache memoryCache)
    {
        Cache = memoryCache;
    }

    public void Add(string filename, string contentType, byte[] content)
    {
        var cacheFile = new CacheFile
        {
            Content = content,
            ContentType = contentType
        };

        lock (Locker)
        {
            Cache.Set(filename, cacheFile, FileExpirationAt);
        }
    }

    public void AddSasLink(string filename, string link, BlobSasBuilder builder)
    {
        var cacheKey = $"[SAS]({filename})";

        lock (Locker)
        {
            Cache.Set(cacheKey, link, builder.ExpiresOn);
        }
    }

    public bool TryGet(string filename, out string contentType, out byte[] content)
    {
        content = Array.Empty<byte>();
        contentType = "";

        lock (Locker)
        {
            if (!Cache.TryGetValue(filename, out CacheFile? cacheFile))
                return false;

            content = cacheFile!.Content;
            contentType = cacheFile.ContentType;
            return true;
        }
    }

    public bool TryGetSasLink(string filename, [MaybeNullWhen(false)] out string link)
    {
        link = null;
        var cacheKey = $"[SAS]({filename})";

        lock (Locker)
        {
            return Cache.TryGetValue(cacheKey, out link);
        }
    }

    public void Remove(string filename)
    {
        lock (Locker)
        {
            Cache.Remove(filename);
        }
    }

    public void RemoveSasLink(string filename)
    {
        lock (Locker)
        {
            Cache.Remove($"[SAS]{filename}");
        }
    }
}
