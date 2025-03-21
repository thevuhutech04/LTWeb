namespace WebApp.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string webRootPath);
        void DeleteFile(string filePath, string webRootPath);
        bool IsValidImage(IFormFile file);
    }
} 