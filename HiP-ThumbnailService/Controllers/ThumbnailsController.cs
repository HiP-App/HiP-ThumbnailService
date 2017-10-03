using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private SizeConfig _sizeConfig;

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

            var folderPath = Path.Combine(_uploadConfig.Path, id);

            //clear folder
            if (Directory.Exists(folderPath))
            {
                var folder = new DirectoryInfo(folderPath);
                var subfolders = folder.EnumerateDirectories();
                //Delete all files before deleting the directories
                foreach (var subfolder in subfolders)
                {
                    DeleteFilesForFolder(subfolder.EnumerateFiles());
                    subfolder.Delete();
                }
                DeleteFilesForFolder(folder.EnumerateFiles());
                folder.Delete(true);

                //Delete all files in a specific folder
                void DeleteFilesForFolder(IEnumerable<FileInfo> files)
                {
                    foreach (var f in folder.GetFiles())
                    {
                        f.Delete();
                    }
                }

            }

            var filePath = Path.Combine(folderPath, Path.GetFileName(file.FileName));
            if (file.Length > 0)
            {
                Directory.CreateDirectory(folderPath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return NoContent();
        }

        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        [HttpGet("{id}")]
        public IActionResult Get(string id, string size, CropMode mode, RequestedImageFormat imageFormat)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var folderPath = Path.Combine(_uploadConfig.Path, id);
            if (!Directory.Exists(folderPath))
                return BadRequest(new { Message = "No image was found for this id" });

            var folder = new DirectoryInfo(folderPath);
            var file = folder.EnumerateFiles().FirstOrDefault();
            if (file == null) return BadRequest();

            var subfolder = folder.GetDirectories().FirstOrDefault(d => d.Name.Equals(SubFolderName)) ??
                Directory.CreateDirectory(Path.Combine(folder.FullName, SubFolderName));

            if (!_sizeConfig.SupportedSizes.ContainsKey(size)) return BadRequest(new { Message = "Invalid size" });

            var extension = RequestImageFormatUtils.GetExtension(imageFormat);
            var thumbnailPath = Path.Combine(subfolder.FullName, GetFileName(size, mode)) + "." + extension;

            var value = _sizeConfig.SupportedSizes[size];
            SaveThumbnail(mode, file.FullName, thumbnailPath, value);

            return File(new FileStream(thumbnailPath, FileMode.Open), $"image/{extension}", Path.GetFileName(thumbnailPath));
        }

        private static void SaveThumbnail(CropMode mode, string filename, string thumbnailPath, int value)
        {
            if (!System.IO.File.Exists(thumbnailPath))
            {
                switch (mode)
                {
                    case CropMode.FillSquare:

                        using (var image = Image.Load(filename))
                        {
                            image.Mutate(c =>
                                c.Resize(new ResizeOptions() { Mode = ResizeMode.Crop, Size = new Size(value) }));
                            image.Save(thumbnailPath);
                        }
                        break;
                    case CropMode.Uniform:
                        using (var image = Image.Load(filename))
                        {
                            image.Mutate(c =>
                                c.Resize(new ResizeOptions() { Mode = ResizeMode.Max, Size = new Size(value) }));
                            image.Save(thumbnailPath);
                        }
                        break;
                }
            }
        }

        string GetFileName(string size, CropMode mode) => $"{size}({mode})";

    }

}
