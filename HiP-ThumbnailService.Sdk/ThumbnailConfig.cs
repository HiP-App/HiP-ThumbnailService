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
        
        public string GetThumbnailUrl(string relativeImageUrl) =>
            $"{ThumbnailServiceHost}/api/Thumbnails?Url={relativeImageUrl}";
    }
}
