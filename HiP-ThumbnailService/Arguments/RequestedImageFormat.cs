using System;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments
{
    public enum RequestedImageFormat
    {
        Jpeg, Png

    }

    public class RequestImageFormatUtils
    {
        public static string GetExtension(RequestedImageFormat format)
        {
            string extension;
            switch (format)
            {
                case RequestedImageFormat.Jpeg:
                    extension = "jpg";
                    break;
                case RequestedImageFormat.Png:
                    extension = "png";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unexpected image format");
            }
            return extension;

        }
    }

}
