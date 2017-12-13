using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService
{
    /// <summary>
    /// A service that can be used with ASP.NET Core dependency injection.
    /// Usage: In ConfigureServices():
    /// <code>
    /// services.Configure&lt;ThumbnailConfig&gt;(Configuration.GetSection("Thumbnails"));
    /// services.AddSingleton&lt;ThumbnailService&gt;();
    /// </code>
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

        /// <summary>
        /// Constructs an absolute URL that, when accessed, returns an image from the thumbnail service.
        /// The specified arguments are inserted into the configured <see cref="ThumbnailConfig.ThumbnailUrlPattern"/>.
        /// 
        /// Example: Given
        /// ThumbnailServiceHost = "https://docker-hip.cs.upb.de/develop/thumbnailservice" and
        /// ThumbnailUrlPattern = "datastore/api/Media/{0}/File",
        /// => GetThumbnailUrl(42) = "https://docker-hip.cs.upb.de/develop/thumbnailservice?Url=datastore/api/Media/42/File"
        /// </summary>
        public string GetThumbnailUrl(params string[] args) =>
            $"{_config.ThumbnailServiceHost}/api/Thumbnails?Url={GetThumbnailUrlArgument(args)}";

        /// <summary>
        /// Constructs the relative URL that is used to request thumbnails by inserting the
        /// specified arguments into the configured <see cref="ThumbnailConfig.ThumbnailUrlPattern"/>.
        /// </summary>
        public string GetThumbnailUrlArgument(params string[] args) =>
            string.Format(_config.ThumbnailUrlPattern ?? "", args);
    }
}
