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
        private readonly ThumbnailConfig _thumbnailConfig;

        /// <summary>
        /// This dictionary is used for synchronizaing requests for the same id
        /// </summary>
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> LockDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ThumbnailsController(IOptions<ThumbnailConfig> thumbnailConfig)
        {
            _thumbnailConfig = thumbnailConfig.Value;
        }

        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [HttpDelete]
        public async Task<IActionResult> Delete(string url)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var encodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(url));
            var folderPath = Path.Combine(_thumbnailConfig.Path, encodedId);

            var semaphore = LockDictionary.GetOrAdd(encodedId, new SemaphoreSlim(1));

            await semaphore.WaitAsync();
            try
            {
                //delete the folder
                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);
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

            if (string.IsNullOrEmpty(args.Url))
                return BadRequest(new { Message = "An url for image access must be provided" });

            var encodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(args.Url));
            var folderPath = Path.Combine(_thumbnailConfig.Path, encodedId);

            if (string.IsNullOrEmpty(args.Size) || !_thumbnailConfig.SupportedSizes.ContainsKey(args.Size))
                return BadRequest(new { Message = "Invalid size" });

            var semaphore = LockDictionary.GetOrAdd(encodedId, new SemaphoreSlim(1));
            await semaphore.WaitAsync();
            try
            {
                var extension = args.Format.GetExtension();
                var thumbnailPath = Path.Combine(folderPath, GetFileName(args.Size, args.Mode)) + "." +
                                    extension;
                if (!System.IO.File.Exists(thumbnailPath))
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", Request.Headers["Authorization"].ToString());
                    var hostUrl = _thumbnailConfig.HostUrl;
                    var stream = await client.GetStreamAsync(hostUrl + args.Url);

                    //Create Directory if it doesn't exist
                    Directory.CreateDirectory(folderPath);

                    var value = _thumbnailConfig.SupportedSizes[args.Size];
                    GenerateThumbnail(args.Mode, stream, thumbnailPath, value);
                }

                return File(new FileStream(thumbnailPath, FileMode.Open), $"image/{extension}",
                    Path.GetFileName(thumbnailPath));

            }
            catch (HttpRequestException)
            {
                return NotFound(new { Message = "Image access failed" });
            }
            finally
            {
                semaphore.Release();
            }

        }

        /// <summary>
        /// Generates a thumbnail
        /// </summary>
        /// <param name="mode">Crop mode</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="thumbnailPath">Path to thumbnail</param>
        /// <param name="value">Size value</param>
        private static void GenerateThumbnail(CropMode mode, Stream fileStream, string thumbnailPath, int value)
        {
            ResizeMode resizeMode = ResizeMode.Crop;
            switch (mode)
            {

                case CropMode.FillSquare:
                    resizeMode = ResizeMode.Crop;
                    break;
                case CropMode.Uniform:
                    resizeMode = ResizeMode.Max;
                    break;
            }

            using (var image = Image.Load(fileStream))
            {
                image.Mutate(c =>
                    c.Resize(new ResizeOptions { Mode = resizeMode, Size = new Size(value) }));
                image.Save(thumbnailPath);
            }
        }

        private static string GetFileName(string size, CropMode mode) => $"{size}({mode})";

    }

}
