using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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
        private readonly ILogger<ThumbnailService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ThumbnailsClient ThumbnailsClient => new ThumbnailsClient(_config.ThumbnailServiceHost)
        {
            Authorization = _httpContextAccessor.HttpContext.Request.Headers["Authorization"]
        };

        public ThumbnailService(IOptions<ThumbnailConfig> config, ILogger<ThumbnailService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _config = config.Value;
            _logger = logger;
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

        /// <summary>
        /// Tries to delete all cached thumbnails of an image in the thumbnail service.
        /// Exceptions are catched and logged as warning.
        /// </summary>
        public async Task<bool> TryClearThumbnailCacheAsync(params string[] args)
        {
            if (string.IsNullOrWhiteSpace(_config.ThumbnailUrlPattern) ||
                string.IsNullOrWhiteSpace(_config.ThumbnailServiceHost))
            {
                return false;
            }

            try
            {
                await ThumbnailsClient.DeleteAsync(GetThumbnailUrlArgument(args));
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e,
                    $"Request to clear thumbnail cache failed for media '{id}'; " +
                    $"thumbnail service might return outdated images (request URL was '{url}').");

                return false;
            }
        }
    }
}
