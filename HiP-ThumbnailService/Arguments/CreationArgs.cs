using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments
{
    public class CreationArgs
    {
        public string Url { get; set; }
        
        public string Size { get; set; }

        /// <summary>
        /// Specifies crop mode that is used. If no mode is specified <value>FillSquare</value> is used
        /// </summary>
        public CropMode Mode { get; set; } = CropMode.FillSquare;


        /// <summary>
        /// Specifies image format that the resulting thumbnail has. If no format is specified <value>Jpeg</value> is used
        /// </summary>
        public RequestedImageFormat Format { get; set; } = RequestedImageFormat.Jpeg;
    }
}
