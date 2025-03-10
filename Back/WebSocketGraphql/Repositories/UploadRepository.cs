using Microsoft.Extensions.Options;
using WebSocketGraphql.Configurations;
using WebSocketGraphql.Helpers;

namespace WebSocketGraphql.Repositories
{
    public class UploadRepository : IUploadRepository
    {
        private readonly string _uploadDirectory;
        private readonly Random _rnd = new();
        private readonly string _smallPostfix;
        private readonly long _maxImageSize;
        private readonly int _smallImageSize;

        public UploadRepository(IWebHostEnvironment env, IOptions<GeneralSettings> settings)
        {
            _uploadDirectory = env.WebRootPath;
            _smallPostfix = settings.Value.SmallImagePostfix;
            _maxImageSize = settings.Value.MaxPictureSizeInKB * 1024;
            _smallImageSize = settings.Value.SmallImageSize;
        }

        public void DeleteFileWithSmallOne(string relatingPath)
        {
            File.Delete(Path.Combine(_uploadDirectory, relatingPath));
            var file = Path.GetFileNameWithoutExtension(relatingPath);
            var NewPath = relatingPath.Replace(file, file + _smallPostfix);
            File.Delete(Path.Combine(_uploadDirectory, NewPath));
        }

        public void DeleteFile(string relatingPath)
        {
            File.Delete(Path.Combine(_uploadDirectory, relatingPath));
        }

        public async Task<string> SaveImgAsync(IFormFile formFile, string catalog, bool ignoreValidation = false)
        {
            if (ignoreValidation && formFile.Length > _maxImageSize)
                throw new InvalidDataException("You reached maximum size value of file");

            if (!formFile.ContentType.StartsWith("image"))
                throw new InvalidDataException("File is not a image");

            var id = Guid.NewGuid().ToString().Replace('-', (char)(_rnd.Next(26) + 65));
            var categoryFolderPath = Path.Combine(_uploadDirectory, catalog);

            if (!Directory.Exists(categoryFolderPath))
                Directory.CreateDirectory(categoryFolderPath);

            var path = id + Path.GetExtension(formFile.FileName);
            var safePath = Path.Combine(categoryFolderPath, path);

            using var fs = formFile.OpenReadStream();
            using var ws = File.Create(safePath);

            await fs.CopyToAsync(ws).ConfigureAwait(false);

            return path;
        }

        public async Task<string> SaveImgWithSmallOneAsync(IFormFile formFile, string catalog, bool ignoreValidation = false)
        {
            var mainLink = await SaveImgAsync(formFile, catalog, ignoreValidation);
            using var memoryStream = new MemoryStream();

            try
            {
                await formFile.CopyToAsync(memoryStream);
                var smallPict = ImageHelper.ScaleImage(memoryStream.ToArray(), _smallImageSize, _smallImageSize);

                var extension = Path.GetExtension(mainLink);
                var safePath = Path.Combine(_uploadDirectory, catalog, $"{Path.GetFileNameWithoutExtension(mainLink)}{_smallPostfix}{extension}");

                using var ws = File.Create(safePath);

                await ws.WriteAsync(smallPict);

            }
            catch
            {
                if (mainLink is not null)
                    DeleteFile(Path.Combine(catalog, mainLink));

                throw;
            }

            return mainLink;
        }

    }
}
