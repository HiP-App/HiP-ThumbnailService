using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Utility
{
    public class UploadFilesConfig
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
