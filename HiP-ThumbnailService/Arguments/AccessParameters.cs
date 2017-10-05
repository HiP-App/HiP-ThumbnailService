using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments
{
    public class AccessParameters
    {
        [Required]
        public string Url { get; set; }

    }
}
