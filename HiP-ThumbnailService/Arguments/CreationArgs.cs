namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments
{
    public class CreationArgs
    {
        public string Url { get; set; }
        
        public string Size { get; set; }

        /// <summary>
        /// Specifies crop mode that is used. If no mode is specified <see cref="CropMode.FillSquare"/> is used
        /// </summary>
        public CropMode Mode { get; set; } = CropMode.FillSquare;


        /// <summary>
        /// Specifies image format that the resulting thumbnail has. If no format is specified <see cref="RequestedImageFormat.Jpeg"/> is used
        /// </summary>
        public RequestedImageFormat Format { get; set; } = RequestedImageFormat.Jpeg;
    }
}
