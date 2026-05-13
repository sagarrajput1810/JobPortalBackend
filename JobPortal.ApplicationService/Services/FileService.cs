using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace JobPortal.ApplicationService.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string subFolder);
        void DeleteFile(string filePath);
    }

    public class LocalFileService : IFileService
    {
        private readonly string _contentRootPath;

        public LocalFileService(string contentRootPath)
        {
            _contentRootPath = contentRootPath;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is required.", nameof(file));
            }

            // Folder path set karein (e.g., wwwroot/uploads/resumes)
            string uploadFolder = Path.Combine(_contentRootPath, "wwwroot", "uploads", subFolder);
            
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Unique file name banayein
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Database mein save karne ke liye relative URL return karein
            return $"/uploads/{subFolder}/{fileName}";
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            
            string fullPath = Path.Combine(_contentRootPath, "wwwroot", filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
