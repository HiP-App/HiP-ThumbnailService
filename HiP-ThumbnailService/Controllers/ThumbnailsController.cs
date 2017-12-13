using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.ThumbnailService.Arguments;
using PaderbornUniversity.SILab.Hip.ThumbnailService.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ThumbnailsController : Controller
    {
        private readonly ThumbnailConfig _thumbnailConfig;

        // This dictionary is used for synchronizing requests for the same id
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> LockDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ThumbnailsController(IOptions<ThumbnailConfig> thumbnailConfig)
        {
            _thumbnailConfig = thumbnailConfig.Value;
        }

        /// <summary>
        /// Clears the thumbnail cache for the specified URL.
        /// </summary>
        /// <param name="url">URL relative to 'HostUrl' configured in the thumbnail service.</param>
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
                // delete the folder
                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);
                return NoContent();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Gets the thumbnail for the specififed URL with the specified parameters (size, format etc.)
        /// </summary>
        /// <param name="args">Arguments for Thumbnail creation</param>
        /// <returns></returns>
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(FileStream), 200)]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]CreationArgs args)
        {
            if (!string.IsNullOrEmpty(args.Size) && !_thumbnailConfig.SupportedSizes.ContainsKey(args.Size))
                ModelState.AddModelError(nameof(args.Size), "Invalid size. Must be one of the following: " +
                    string.Join(", ", _thumbnailConfig.SupportedSizes));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var encodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(args.Url));
            var folderPath = Path.Combine(_thumbnailConfig.Path, encodedId);

            var semaphore = LockDictionary.GetOrAdd(encodedId, new SemaphoreSlim(1));
            await semaphore.WaitAsync();
            try
            {
                var requestedImageFormat = args.Format.GetImageFormat();
                var filePath = Path.Combine(folderPath, GetFileName(args.Size, args.Mode)) + "." +
                               requestedImageFormat.FileExtensions.First();
                if (!System.IO.File.Exists(filePath))
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", Request.Headers["Authorization"].ToString());

                    using (var stream = await client.GetStreamAsync(_thumbnailConfig.HostUrl + args.Url))
                    {
                        // Create directory if it doesn't exist
                        Directory.CreateDirectory(folderPath);

                        if (string.IsNullOrEmpty(args.Size))
                        {
                            // The original image should be returned if the size is empty
                            SaveImage(stream, filePath);
                        }
                        else
                        {
                            var value = _thumbnailConfig.SupportedSizes[args.Size];
                            GenerateThumbnail(args.Mode, stream, filePath, value);
                        }
                    }
                }

                return File(new FileStream(filePath, FileMode.Open), requestedImageFormat.DefaultMimeType,
                    Path.GetFileName(filePath));

            }
            catch (HttpRequestException)
            {
                return NotFound(new { Message = "Image access failed" });
            }
            catch (NotSupportedException)
            {
                return BadRequest(new { Message = "The format of the requested image is not supported" });
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Generates a new thumbnail which replaces the old thumbnail.
        /// </summary>
        /// <param name="mode">Crop mode</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="thumbnailPath">Path to thumbnail</param>
        /// <param name="size">Size value</param>
        private static void GenerateThumbnail(CropMode mode, Stream fileStream, string thumbnailPath, int size)
        {
            var resizeMode = ResizeMode.Crop;

            switch (mode)
            {
                case CropMode.FillSquare:
                    resizeMode = ResizeMode.Crop;
                    break;
                case CropMode.Uniform:
                    resizeMode = ResizeMode.Max;
                    break;
            }

            SaveImage(fileStream, thumbnailPath,
                c => c.Resize(new ResizeOptions { Mode = resizeMode, Size = new Size(size) }));
        }

        private static void SaveImage(Stream fileStream, string filePath, Action<IImageProcessingContext<Rgba32>> operation = null)
        {
            using (var image = Image.Load(fileStream))
            {
                if (operation != null)
                    image.Mutate(operation);

                image.Save(filePath);
            }
        }

        private static string GetFileName(string size, CropMode mode) =>
            string.IsNullOrEmpty(size) ? "originalImage" : $"{size}({mode})";
    }
}
