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
            Authorization = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
        };

        public ThumbnailService(IOptions<ThumbnailConfig> config, ILogger<ThumbnailService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _config = config.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            if (string.IsNullOrWhiteSpace(config.Value.ThumbnailServiceHost))
                logger.LogWarning($"{nameof(ThumbnailConfig.ThumbnailServiceHost)} is not configured correctly!");

            if (string.IsNullOrWhiteSpace(config.Value.ThumbnailUrlPattern))
                logger.LogWarning($"{nameof(ThumbnailConfig.ThumbnailUrlPattern)} is not configured correctly!");
        }

        /// <summary>
        /// Constructs an absolute URL that, when accessed, returns an image from the thumbnail service.
        /// 
        /// Example: Given
        /// ThumbnailServiceHost = "https://docker-hip.cs.upb.de/develop/thumbnailservice" and
        /// ThumbnailUrlPattern = "datastore/api/Media/{0}/File",
        /// => GetThumbnailUrl(42) = "https://docker-hip.cs.upb.de/develop/thumbnailservice?Url=datastore/api/Media/42/File"
        /// </summary>
        /// <param name="args">Arguments replacing the placeholders in <see cref="ThumbnailConfig.ThumbnailUrlPattern"/></param>
        public string GetThumbnailUrl(params object[] args) =>
            $"{_config.ThumbnailServiceHost}/api/Thumbnails?Url={GetThumbnailUrlArgument(args)}";

        /// <summary>
        /// Constructs the relative URL that is used to request thumbnails.
        /// </summary>
        /// <param name="args">Arguments replacing the placeholders in <see cref="ThumbnailConfig.ThumbnailUrlPattern"/></param>
        public string GetThumbnailUrlArgument(params object[] args) =>
            string.Format(_config.ThumbnailUrlPattern ?? "", args);

        /// <summary>
        /// Tries to delete all cached thumbnails of an image in the thumbnail service.
        /// Exceptions are catched and logged as warning.
        /// </summary>
        /// <param name="args">Arguments replacing the placeholders in <see cref="ThumbnailConfig.ThumbnailUrlPattern"/></param>
        public async Task<bool> TryClearThumbnailCacheAsync(params object[] args)
        {
            if (string.IsNullOrWhiteSpace(_config.ThumbnailUrlPattern) ||
                string.IsNullOrWhiteSpace(_config.ThumbnailServiceHost))
            {
                return false;
            }

            var urlArgument = GetThumbnailUrlArgument(args);

            try
            {
                await ThumbnailsClient.DeleteAsync(urlArgument);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e,
                    $"Request to clear thumbnail cache failed for relative URL '{urlArgument}'; " +
                    $"thumbnail service might return outdated images.");

                return false;
            }
        }
    }
}
