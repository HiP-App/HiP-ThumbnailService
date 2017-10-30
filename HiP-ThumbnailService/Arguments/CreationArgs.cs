using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments
{
    public class CreationArgs
    {
        [Required]
        public string Url { get; set; }

        public string Size { get; set; }

        /// <summary>
        /// Specifies crop mode that is used. If no mode is specified <see cref="CropMode.FillSquare"/> is used
        /// </summary>
        [JsonIgnore]
        public CropMode Mode
        {
#pragma warning disable 0618
            get => InternalMode ?? CropMode.FillSquare;
            set => InternalMode = value;
#pragma warning restore 0618
        }

        /// Property 'InternalMode' is listed by NSwag, but not by Swashbuckle (due to 'internal').
        /// Property 'Mode' is listed by Swashbuckle, but not by NSwag (due to [JsonIgnore]).
        /// 
        /// We need a NULLABLE status parameter for NSwag because:
        /// 1) The status parameter shouldn't be required (clients shouldn't need to pass it, it defaults to FillSquare)
        /// 2) Since status is not required, the NSwag-generated C# client has "CropMode? mode = null" in the
        ///    method signature, however if it weren't nullable here, the client would throw an exception if 'mode == null'.
        ///    This is weird: The method signature states that status can be null, but passing null throws an exception.
        ///    
        /// Why don't we make Mode nullable in general? We don't want the rest of the codebase to have to distinguish
        /// between 'Mode == null' and 'Mode == FillSquare'.
        [JsonProperty("mode")]
        [DefaultValue(CropMode.FillSquare)]
        [Obsolete("For internal use only. Use 'Mode' instead.")]
        internal CropMode? InternalMode { get; set; }

        /// <summary>
        /// Specifies image format that the resulting thumbnail has. If no format is specified <see cref="RequestedImageFormat.Jpeg"/> is used
        /// </summary>
        [JsonIgnore]
        public RequestedImageFormat Format
        {
#pragma warning disable 0618
            get => InternalFormat ?? RequestedImageFormat.Jpeg;
            set => InternalFormat = value;
#pragma warning restore 0618
        }

        /// Property 'InternalFormat' is listed by NSwag, but not by Swashbuckle (due to 'internal').
        /// Property 'Format' is listed by Swashbuckle, but not by NSwag (due to [JsonIgnore]).
        /// 
        /// We need a NULLABLE status parameter for NSwag because:
        /// 1) The status parameter shouldn't be required (clients shouldn't need to pass it, it defaults to Jpeg)
        /// 2) Since status is not required, the NSwag-generated C# client has "RequestedImageFormat? format = null" in the
        ///    method signature, however if it weren't nullable here, the client would throw an exception if 'format == null'.
        ///    This is weird: The method signature states that status can be null, but passing null throws an exception.
        ///    
        /// Why don't we make Format nullable in general? We don't want the rest of the codebase to have to distinguish
        /// between 'Format == null' and 'Format == Jpeg'.
        [JsonProperty("format")]
        [DefaultValue(RequestedImageFormat.Jpeg)]
        [Obsolete("For internal use only. Use 'Format' instead.")]
        internal RequestedImageFormat? InternalFormat { get; set; }
    }
}
