using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp.Services
{
    public class FileService : IFileService
    {
        private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const int MaxFileSizeInMB = 5;
        private const int MaxImageDimension = 2000;

        public async Task<string> SaveFileAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ");

            if (!IsValidImage(file))
                throw new ArgumentException("File không phải là ảnh hợp lệ");

            var uploadsFolder = Path.Combine(webRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = GetUniqueFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/" + uniqueFileName;
        }

        public void DeleteFile(string filePath, string webRootPath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            var fullPath = Path.Combine(webRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Kiểm tra kích thước file
            if (file.Length > MaxFileSizeInMB * 1024 * 1024)
                return false;

            // Kiểm tra phần mở rộng
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return false;

            // Kiểm tra nội dung file
            try
            {
                using (var stream = file.OpenReadStream())
                using (var image = Image.FromStream(stream))
                {
                    // Kiểm tra kích thước ảnh
                    if (image.Width > MaxImageDimension || image.Height > MaxImageDimension)
                        return false;

                    // Kiểm tra format ảnh
                    return image.RawFormat.Guid == ImageFormat.Jpeg.Guid ||
                           image.RawFormat.Guid == ImageFormat.Png.Guid ||
                           image.RawFormat.Guid == ImageFormat.Gif.Guid;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_"
                   + Guid.NewGuid().ToString().Substring(0, 8)
                   + Path.GetExtension(fileName);
        }
    }
} 