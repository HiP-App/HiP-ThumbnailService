using System;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments
{
    public enum RequestedImageFormat
    {
        Jpeg, Png

    }

    public static class RequestImageFormatUtils
    {
        public static string GetExtension(this RequestedImageFormat format)
        {
            switch (format)
            {
                case RequestedImageFormat.Jpeg:
                    return "jpeg";
                case RequestedImageFormat.Png:
                    return "png";
                default:
                    throw new ArgumentOutOfRangeException("Unexpected image format");
            }

        }
    }

}
