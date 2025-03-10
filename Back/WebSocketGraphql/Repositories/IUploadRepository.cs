namespace WebSocketGraphql.Repositories
{
    public interface IUploadRepository
    {
        Task<string> SaveImgAsync(IFormFile formFile, string catalog, bool ignoreValidation = false);

        Task<string> SaveImgWithSmallOneAsync(IFormFile formFile, string catalog, bool ignoreValidation = false);
        
        void DeleteFile(string relatingPath);

        void DeleteFileWithSmallOne(string relatingPath);
    }
}
