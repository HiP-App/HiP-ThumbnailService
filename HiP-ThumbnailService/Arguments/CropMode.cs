namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments
{
    /// <summary>
    /// Describes the different modes that can be used for cropping.
    /// <see cref="FillSquare"/> crops at the center of the image resulting in a square.
    /// <see cref="Uniform"/> keeps the aspect ratio while cropping the longer side to the specified value.
    /// </summary>
    public enum CropMode
    {
        FillSquare, Uniform
    }
}
