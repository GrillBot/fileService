using Microsoft.Extensions.Caching.Memory;

namespace FileService.Cache;

public class StorageCacheManager
{
    private IMemoryCache Cache { get; }

    private static readonly object Locker = new();

    private static DateTimeOffset ExpirationAt
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
            Cache.Set(filename, cacheFile, ExpirationAt);
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

    public void Remove(string filename)
    {
        lock (Locker)
        {
            Cache.Remove(filename);
        }
    }
}
