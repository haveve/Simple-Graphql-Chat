using CourseWorkDB.Repositories;
using System;
using System.Collections.Concurrent;

namespace FileUploadSample
{
    public class UploadRepository: IUploadRepository
    {
        private readonly string _uploadDirectory;
        private readonly Random _rnd;

        public UploadRepository(IWebHostEnvironment env)
        {
            _uploadDirectory = env.WebRootPath;
            _rnd = new Random();
        }

        public void DeleteFile(string relatingPath)
        {
            File.Delete(Path.Combine(_uploadDirectory,relatingPath));
        }

        public async Task<string> SaveImgAsync(IFormFile formFile, string catalog)
        {
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

    }
}
