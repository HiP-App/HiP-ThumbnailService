using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments;
using PaderbornUniversity.SILab.Hip.ThumbnailService.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ThumbnailsController : Controller
    {
        private readonly DirConfig _dirConfig;
        private readonly SizeConfig _sizeConfig;
        private readonly EndpointConfig _endpointConfig;

        /// <summary>
        /// This dictionary is used for synchronizaing requests for the same id
        /// </summary>
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> LockDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ThumbnailsController(IOptions<DirConfig> uploadConfig, IOptions<SizeConfig> sizeConfig, IOptions<EndpointConfig> endpointConfig)
        {
            _dirConfig = uploadConfig.Value;
            _sizeConfig = sizeConfig.Value;
            _endpointConfig = endpointConfig.Value;
        }

        [ProducesResponseType(204)]
        [HttpDelete]
        public async Task<IActionResult> Put(string url)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            

            var encodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(url));
            var folderPath = Path.Combine(_dirConfig.Path, encodedId);

            var semaphore = LockDictionary.GetOrAdd(encodedId, new SemaphoreSlim(1));

            await semaphore.WaitAsync();
            try
            {
                //clear folder
                if (Directory.Exists(folderPath))
                {
                    var folder = new DirectoryInfo(folderPath);
                    folder.Delete(true);
                }
                
                return NoContent();
            }
            finally
            {
                semaphore.Release();
            }
        }

        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        [HttpGet]
        public async Task<IActionResult> Get(CreationArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(args.Url)) return BadRequest(new { Message = "An url for image access must be provided" });

            var encodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(args.Url));
            var folderPath = Path.Combine(_dirConfig.Path, encodedId);

            if (string.IsNullOrEmpty(args.Size) || !_sizeConfig.SupportedSizes.ContainsKey(args.Size))
                return BadRequest(new { Message = "Invalid size" });

            var semaphore = LockDictionary.GetOrAdd(encodedId, new SemaphoreSlim(1));
            await semaphore.WaitAsync();
            try
            {
                var extension = RequestImageFormatUtils.GetExtension(args.Format);
                var thumbnailPath = Path.Combine(folderPath, GetFileName(args.Size, args.Mode)) + "." +
                                    extension;
                if (!System.IO.File.Exists(thumbnailPath))
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", Request.Headers["Authorization"].ToString());
                    var hostUrl = _endpointConfig.HostUrl;
                    var stream = await client.GetStreamAsync(hostUrl + args.Url);

                    if (stream == null)
                    {
                        return BadRequest(new { Message = "Image access failed" });
                    }

                    //Create Directory if it doesn't exist
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var value = _sizeConfig.SupportedSizes[args.Size];
                    GenerateThumbnail(args.Mode, stream, thumbnailPath, value);
                }

                return File(new FileStream(thumbnailPath, FileMode.Open), $"image/{extension}",
                        Path.GetFileName(thumbnailPath));

            }
            finally
            {
                semaphore.Release();
            }

        }

        /// <summary>
        /// Generates a thumbnail, if it doesn't exist yet
        /// </summary>
        /// <param name="mode">Crop mode</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="thumbnailPath">Path to thumbnail</param>
        /// <param name="value">Size value</param>
        private static void GenerateThumbnail(CropMode mode, Stream fileStream, string thumbnailPath, int value)
        {
            switch (mode)
            {
                case CropMode.FillSquare:

                    using (var image = Image.Load(fileStream))
                    {
                        image.Mutate(c =>
                            c.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Size = new Size(value) }));
                        image.Save(thumbnailPath);
                    }
                    break;
                case CropMode.Uniform:
                    using (var image = Image.Load(fileStream))
                    {
                        image.Mutate(c =>
                            c.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(value) }));
                        image.Save(thumbnailPath);
                    }
                    break;
            }

        }

        string GetFileName(string size, CropMode mode) => $"{size}({mode})";

    }

}
