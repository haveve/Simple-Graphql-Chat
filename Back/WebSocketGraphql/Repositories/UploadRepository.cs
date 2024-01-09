using CourseWorkDB.Repositories;
using System;
using System.Collections.Concurrent;

namespace FileUploadSample
{
    public class UploadRepository : IUploadRepository
    {
        private readonly string _uploadDirectory;
        private readonly Random _rnd;

        public UploadRepository(IWebHostEnvironment env)
        {
            _uploadDirectory = env.WebRootPath;
            _rnd = new Random();
        }

        public void DeleteFileWithSmallOne(string relatingPath, string smallPostfix = IUploadRepository.defaultSmallOnePostfix)
        {
            File.Delete(Path.Combine(_uploadDirectory, relatingPath));
            string file = Path.GetFileNameWithoutExtension(relatingPath);
            string NewPath = relatingPath.Replace(file, file + smallPostfix);
            File.Delete(Path.Combine(_uploadDirectory, NewPath));
        }

        public void DeleteFile(string relatingPath)
        {
            File.Delete(Path.Combine(_uploadDirectory, relatingPath));
        }

        public async Task<string> SaveImgAsync(IFormFile formFile, string catalog, int maxFileSizeInKB = 0)
        {

            if (maxFileSizeInKB > 0 && formFile.Length > maxFileSizeInKB * 1024)
            {
                throw new InvalidDataException("You reached maximum size value of file");
            }

            if (!formFile.ContentType.StartsWith("image"))
            {
                throw new InvalidDataException("File is not a image");
            }

            var id = Guid.NewGuid().ToString().Replace('-', (char)(_rnd.Next(26) + 65));
            var categoryFolderPath = Path.Combine(_uploadDirectory, catalog);


            if (!Directory.Exists(categoryFolderPath))
            {
                Directory.CreateDirectory(categoryFolderPath);
            }

            var path = id + Path.GetExtension(formFile.FileName);
            var safePath = Path.Combine(categoryFolderPath, path);

            using (var fs = formFile.OpenReadStream())
            using (var ws = System.IO.File.Create(safePath))
            {
                await fs.CopyToAsync(ws).ConfigureAwait(false);
            }

            return path;
        }

        public async Task<string> SaveImgWithSmallOneAsync(IFormFile formFile, string catalog, int smallWidth, int smallHeigh, string smallPostfix = IUploadRepository.defaultSmallOnePostfix, int maxFileSizeInKB = 0)
        {
            var mainLink = await SaveImgAsync(formFile, catalog, maxFileSizeInKB);
            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    await formFile.CopyToAsync(memoryStream);
                    var smallPict = ImageHelper.ScaleImage(memoryStream.ToArray(), smallWidth, smallHeigh);

                    var extension = Path.GetExtension(mainLink);
                    var safePath = Path.Combine(_uploadDirectory, catalog, Path.GetFileNameWithoutExtension(mainLink) + smallPostfix + extension);
                    using (var ws = System.IO.File.Create(safePath))
                    {
                        await ws.WriteAsync(smallPict);
                    }
                }
                catch
                {
                    if (mainLink is not null)
                    {
                        DeleteFile(Path.Combine(catalog, mainLink));
                    }
                    throw;
                }
            }
            return mainLink;
        }

    }
}
