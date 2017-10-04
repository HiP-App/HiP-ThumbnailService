using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    public class ThumbnailsController : Controller
    {
        private const string SubFolderName = "Thumbnails";
        private readonly UploadFilesConfig _uploadConfig;
        private readonly SizeConfig _sizeConfig;

        /// <summary>
        /// This dictionary is used for synchronizaing requests for the same id
        /// </summary>
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> LockDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ThumbnailsController(IOptions<UploadFilesConfig> uploadConfig, IOptions<SizeConfig> sizeConfig)
        {
            _uploadConfig = uploadConfig.Value;
            _sizeConfig = sizeConfig.Value;
        }

        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, IFormFile file)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ext = Path.GetExtension(file.FileName);
            //Check for supported extensions
            if (!_uploadConfig.SupportedFormats.Contains(ext.ToLower()))
                return BadRequest(new { Message = $"Extension '{ext}' is not supported" });

            var encodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(id));
            var folderPath = Path.Combine(_uploadConfig.Path, encodedId);

            var semaphore = LockDictionary.GetOrAdd(id, new SemaphoreSlim(1));

            await semaphore.WaitAsync();
            try
            {
                //clear folder
                if (Directory.Exists(folderPath))
                {
                    var folder = new DirectoryInfo(folderPath);
                    folder.Delete(true);
                }

                var filePath = Path.Combine(folderPath, Path.GetFileName(file.FileName));
                if (file.Length > 0)
                {
                    var f = Directory.CreateDirectory(folderPath);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
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
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CreationArgs args)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var encodedId = Convert.ToBase64String(Encoding.UTF8.GetBytes(id));
            var folderPath = Path.Combine(_uploadConfig.Path, encodedId);
            if (!Directory.Exists(folderPath))
                return BadRequest(new { Message = "No directory was found for this id" });

            if (string.IsNullOrEmpty(args.Size) || !_sizeConfig.SupportedSizes.ContainsKey(args.Size))
                return BadRequest(new { Message = "Invalid size" });

            var semaphore = LockDictionary.GetOrAdd(id, new SemaphoreSlim(1));
            await semaphore.WaitAsync();
            try
            {
                var folder = new DirectoryInfo(folderPath);
                var file = folder.EnumerateFiles().FirstOrDefault();
                if (file == null)
                {
                    semaphore.Release();
                    return BadRequest(new { Message = "No image was found for this id" });
                }

                var subfolder = folder.GetDirectories().FirstOrDefault(d => d.Name.Equals(SubFolderName)) ??
                                Directory.CreateDirectory(Path.Combine(folder.FullName, SubFolderName));

                var extension = RequestImageFormatUtils.GetExtension(args.Format);
                var thumbnailPath = Path.Combine(subfolder.FullName, GetFileName(args.Size, args.Mode)) + "." +
                                    extension;

                var value = _sizeConfig.SupportedSizes[args.Size];
                GenerateThumbnail(args.Mode, file.FullName, thumbnailPath, value);
                return File(new FileStream(thumbnailPath, FileMode.Open), $"image/{extension}", Path.GetFileName(thumbnailPath));
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
        /// <param name="filename">File name</param>
        /// <param name="thumbnailPath">Path to thumbnail</param>
        /// <param name="value">Size value</param>
        private static void GenerateThumbnail(CropMode mode, string filename, string thumbnailPath, int value)
        {
            if (!System.IO.File.Exists(thumbnailPath))
            {
                switch (mode)
                {
                    case CropMode.FillSquare:

                        using (var image = Image.Load(filename))
                        {
                            image.Mutate(c =>
                                c.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Size = new Size(value) }));
                            image.Save(thumbnailPath);
                        }
                        break;
                    case CropMode.Uniform:
                        using (var image = Image.Load(filename))
                        {
                            image.Mutate(c =>
                                c.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(value) }));
                            image.Save(thumbnailPath);
                        }
                        break;
                }
            }
        }

        string GetFileName(string size, CropMode mode) => $"{size}({mode})";

    }

}
