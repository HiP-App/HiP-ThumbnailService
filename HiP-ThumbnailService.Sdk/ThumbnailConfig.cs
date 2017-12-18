namespace PaderbornUniversity.SILab.Hip.ThumbnailService
{
    /// <summary>
    /// Configuration properties for clients using the thumbnail service.
    /// </summary>
    public sealed class ThumbnailConfig
    {
        /// <summary>
        /// URL pointing to a running instance of the thumbnail service.
        /// Example: "https://docker-hip.cs.upb.de/develop/thumbnailservice"
        /// </summary>
        public string ThumbnailServiceHost { get; set; }

        /// <summary>
        /// Relative URL pattern for generating thumbnail URLs. Should contain one or more placeholders
        /// "{0}", "{1}" etc. that are replaced with the ID(s) of the requested image at runtime.
        /// Example: "datastore/api/Media/{0}/File"
        /// </summary>
        public string ThumbnailUrlPattern { get; set; }
    }
}
