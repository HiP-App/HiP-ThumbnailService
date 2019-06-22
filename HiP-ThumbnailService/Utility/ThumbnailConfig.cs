using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Utility
{
    public class ThumbnailConfig
    {
        /// <summary>
        /// Path where thumbnail images are stored.
        /// Default value: "Thumbnails"
        /// </summary>
        public string Path { get; set; } = "Thumbnails";

        /// <summary>
        /// URL which is a common prefix of all services using the thumbnail service.
        /// Should end with a slash. This prefix is to be omitted when requesting
        /// thumbnails from the service.
        /// Example: "https://docker-hip.cs.upb.de/develop/"
        /// </summary>
        public string HostUrl { get; set; }

        /// <summary>
        /// The image sizes options that can be chosen when requesting thumbnails.
        /// Example: { "small": 50, "medium": 100, "large": 250 }
        /// </summary>
        public Dictionary<string, int> SupportedSizes { get; set; }
    }
}
