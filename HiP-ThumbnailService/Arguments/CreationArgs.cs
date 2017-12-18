using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments
{
    public class CreationArgs
    {
        /// <summary>
        /// URL from where the thumbnail service can retrieve the original image.
        /// This URL must be relative to 'HostUrl' configured in the thumbnail service.
        /// Example: "datastore/Media/42/File"
        /// (with 'HostUrl' configured as "https://docker-hip.cs.upb.de/develop/" for example)
        /// </summary>
        [Required]
        public string Url { get; set; }

        /// <summary>
        /// One of the preconfigured size options, e.g. "small". If null or empty, the image is
        /// returned in its original size without cropping or resizing being applied.
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Image cropping mode that is used. Defaults to <see cref="CropMode.FillSquare"/>.
        /// </summary>
        [DefaultValue(CropMode.FillSquare)]
        public CropMode Mode { get; set; } = CropMode.FillSquare;

        /// <summary>
        /// The desired image format of the resulting thumbnail.
        /// Defaults to <see cref="RequestedImageFormat.Jpeg"/>.
        /// </summary>
        [DefaultValue(RequestedImageFormat.Jpeg)]
        public RequestedImageFormat Format { get; set; } = RequestedImageFormat.Jpeg;
    }
}
