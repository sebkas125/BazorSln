
using BazorProject.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BlazorProject.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesaveController : ControllerBase
    {
        private readonly IWebHostEnvironment env;
        private readonly ILogger<FilesaveController> logger;
        const long MAX_PHOTO_SIZE = 1024 * 512;

        public FilesaveController(IWebHostEnvironment env,
            ILogger<FilesaveController> logger)
        {
            this.env = env;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<IList<UploadResult>>> PostFile(
            [FromForm] IEnumerable<IFormFile> files)
        {
            var maxAllowedFiles = 1;
            long maxUploadFileSize = 1024 * 1024 * 20;
            var filesProcessed = 0;
            var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");
            IList<UploadResult> uploadResults = new List<UploadResult>();

            foreach (var file in files)
            {
                UploadResult uploadResult = new UploadResult();
                string trustedFileNameForFileStorage;
                var untrustedFileName = file.FileName;
                uploadResult.FileName = untrustedFileName;
                string trustedFileNameForDisplay =
                    WebUtility.HtmlEncode(untrustedFileName);

                if (filesProcessed < maxAllowedFiles)
                {
                    if (file.Length == 0)
                    {
                        logger.LogInformation("{FileName} length is 0",
                            trustedFileNameForDisplay);
                        uploadResult.ErrorCode = 1;
                    }
                    else if (file.Length > maxUploadFileSize)
                    {
                        logger.LogInformation("{FileName} of {Length} bytes is " +
                            "larger than the limit of {Limit} bytes",
                            trustedFileNameForDisplay, file.Length, maxUploadFileSize);
                        uploadResult.ErrorCode = 2;
                    }
                    else
                    {
                        try
                        {
                            trustedFileNameForFileStorage = Path.GetRandomFileName().Replace(".", string.Empty) + ".jpg";
                            var path = Path.Combine(env.ContentRootPath, "Files", trustedFileNameForFileStorage);
                            using MemoryStream ms = new();
                            await file.CopyToAsync(ms);
                            await System.IO.File.WriteAllBytesAsync(path, ms.ToArray());
                            if (file.Length > 1024 * 1024)
                            {
                                path = await resizeImage(path);
                            }

                            logger.LogInformation("{FileName} saved at {Path}",
                                trustedFileNameForDisplay, path);
                            uploadResult.Uploaded = true;
                            uploadResult.StoredFileName = trustedFileNameForFileStorage;
                            uploadResult.Path = path;
                        }
                        catch (IOException ex)
                        {
                            logger.LogError("{FileName} error on upload: {Message}",
                                trustedFileNameForDisplay, ex.Message);
                            uploadResult.ErrorCode = 3;
                        }
                    }

                    filesProcessed++;
                }
                else
                {
                    logger.LogInformation("{FileName} not uploaded because the " +
                        "request exceeded the allowed {Count} of files",
                        trustedFileNameForDisplay, maxAllowedFiles);
                    uploadResult.ErrorCode = 4;
                }

                uploadResults.Add(uploadResult);
            }

            return new CreatedResult(resourcePath, uploadResults);
        }

        private async Task<string> resizeImage(string path)
        {
            Image image = Image.FromFile(path);
            MemoryStream stream = DownscaleImage(image);
            path = Path.Combine(Path.GetDirectoryName(path), "Small", $"{Path.GetFileNameWithoutExtension(path)}_smaller.jpg");
            using (var smallFile = System.IO.File.Create(path))
            {
                stream.CopyTo(smallFile);
            }

            return path;
        }

        private MemoryStream DownscaleImage(Image photo)
        {
            MemoryStream resizedPhotoStream = new MemoryStream();

            long resizedSize = 0;
            var quality = 93;
            //long lastSizeDifference = 0;
            do
            {
                resizedPhotoStream.SetLength(0);

                EncoderParameters eps = new EncoderParameters(1);
                eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
                ImageCodecInfo ici = GetEncoderInfo("image/jpeg");

                photo.Save(resizedPhotoStream, ici, eps);
                resizedSize = resizedPhotoStream.Length;

                //long sizeDifference = resizedSize - MAX_PHOTO_SIZE;
                //Console.WriteLine(resizedSize + "(" + sizeDifference + " " + (lastSizeDifference - sizeDifference) + ")");
                //lastSizeDifference = sizeDifference;
                quality--;

            } while (resizedSize > MAX_PHOTO_SIZE);

            resizedPhotoStream.Seek(0, SeekOrigin.Begin);
            return resizedPhotoStream;
        }

        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }

            return null;
        }
    }
}
