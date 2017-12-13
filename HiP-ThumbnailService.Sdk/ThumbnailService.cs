using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService
{
    /// <summary>
    /// A service that can be used with ASP.NET Core dependency injection.
    /// Usage: In ConfigureServices():
    /// services.Configure&lt;ThumbnailConfig&gt;(Configuration.GetSection("Thumbnails"));
    /// services.AddSingleton&lt;ThumbnailService&gt;();
    /// </summary>
    public class ThumbnailService
    {
        private readonly ThumbnailConfig _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ThumbnailsClient Thumbnails => new ThumbnailsClient(_config.ThumbnailServiceHost)
        {
            Authorization = _httpContextAccessor.HttpContext.Request.Headers["Authorization"]
        };
        
        public ThumbnailService(IOptions<ThumbnailConfig> config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config.Value;
            _httpContextAccessor = httpContextAccessor;
        }
    }
}
