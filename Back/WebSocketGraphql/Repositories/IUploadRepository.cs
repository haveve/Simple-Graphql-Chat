namespace CourseWorkDB.Repositories
{
    public interface IUploadRepository
    {
        public const string defaultSmallOnePostfix = "-small";

        Task<string> SaveImgAsync(IFormFile formFile, string catalog, int maxFileSizeInKB = 0);

        Task<string> SaveImgWithSmallOneAsync(IFormFile formFile, string catalog, int smallWidth, int smallHeigh, string smallPostfix = "-small", int maxFileSizeInKB = 0);
        void DeleteFile(string relatingPath);

        void DeleteFileWithSmallOne(string relatingPath, string smallPostfix = defaultSmallOnePostfix);
    }
}
