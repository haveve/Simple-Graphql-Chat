namespace CourseWorkDB.Repositories
{
    public interface IUploadRepository
    {
        Task<string> SaveImgAsync(IFormFile formFile, string catalog, int maxFileSizeInKB = 0);
        void DeleteFile(string relatingPath);
    }
}
