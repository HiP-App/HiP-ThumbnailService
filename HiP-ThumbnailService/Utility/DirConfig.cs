using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Utility
{
    public class DirConfig
    {
        /// <summary>
        /// Path to thumbnail images
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Formats that are supported for file upload
        /// </summary>
        public List<string> SupportedFormats { get; set; }
    }
}
