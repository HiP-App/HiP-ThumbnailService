using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Utility
{
    public class ThumbnailConfig
    {
        /// <summary>
        /// Path to thumbnail images
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Url which describes where the service is hosted. Should end with a slash
        /// </summary>
        public string HostUrl { get; set; }

        /// <summary>
        /// Dictionary for the supported sizes
        /// </summary>
        public Dictionary<string,int> SupportedSizes { get; set; }
    }
}
