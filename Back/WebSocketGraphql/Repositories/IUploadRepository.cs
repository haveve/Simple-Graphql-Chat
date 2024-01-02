namespace CourseWorkDB.Repositories
{
    public interface IUploadRepository
    {
        Task<string> SaveImgAsync(IFormFile formFile, string catalog);
        void DeleteFile(string relatingPath);
    }
}
