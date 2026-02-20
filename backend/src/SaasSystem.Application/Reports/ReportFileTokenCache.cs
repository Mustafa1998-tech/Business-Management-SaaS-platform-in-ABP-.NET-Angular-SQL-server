using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace SaasSystem.Reports;

[CacheName("SaasSystem.ReportExports")]
[Serializable]
public class ReportFileCacheItem
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

public interface IReportFileTokenCache
{
    Task<string> SetAsync(byte[] content);
    Task<ReportFileCacheItem?> GetAsync(string token);
}

public class ReportFileTokenCache : IReportFileTokenCache, ITransientDependency
{
    private readonly IDistributedCache<ReportFileCacheItem, string> _cache;
    private readonly IGuidGenerator _guidGenerator;

    public ReportFileTokenCache(
        IDistributedCache<ReportFileCacheItem, string> cache,
        IGuidGenerator guidGenerator)
    {
        _cache = cache;
        _guidGenerator = guidGenerator;
    }

    public async Task<string> SetAsync(byte[] content)
    {
        string token = _guidGenerator.Create().ToString("N");

        await _cache.SetAsync(
            token,
            new ReportFileCacheItem
            {
                Content = content
            },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

        return token;
    }

    public async Task<ReportFileCacheItem?> GetAsync(string token)
    {
        return await _cache.GetAsync(token);
    }
}
