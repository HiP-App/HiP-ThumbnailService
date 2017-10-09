using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments
{
    public enum RequestedImageFormat
    {
        Jpeg, Png

    }

    public static class RequestImageFormatUtils
    {
        public static IImageFormat GetExtension(this RequestedImageFormat format)
        {
            switch (format)
            {
                case RequestedImageFormat.Jpeg:
                    return ImageFormats.Jpeg;
                case RequestedImageFormat.Png:
                    return ImageFormats.Png;
                default:
                    throw new ArgumentOutOfRangeException("Unexpected image format");
            }

        }
    }

}
