using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace JobPortal.ApplicationService.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string subFolder);
        void DeleteFile(string filePath);
        string GetContentRootPath();
    }

    public class AzureBlobStorageService : IFileService
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureStorage:ConnectionString is missing");
            _containerName = configuration["AzureStorage:ContainerName"] ?? "resumes";
            _logger = logger;
        }

        public string GetContentRootPath() => ""; // Not needed for Blobs

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("File is required.");

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            try
            {
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            }
            catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
            {
                _logger.LogWarning("Public access is not permitted on this storage account. Creating container with private access.");
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            }

            string fileName = $"{subFolder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(fileName);

            _logger.LogInformation("Uploading to Azure Blob Storage: {FileName}", fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            _logger.LogInformation("Upload successful. Blob Uri: {Uri}", blobClient.Uri);
            return blobClient.Uri.ToString(); // Returns absolute URL
        }

        public void DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath)) return;
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var uri = new Uri(filePath);
                var blobClient = new BlobClient(_connectionString, _containerName, uri.AbsolutePath.Replace($"/{_containerName}/", ""));
                blobClient.DeleteIfExists();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete blob: {FilePath}", filePath);
            }
        }
    }

    public class LocalFileService : IFileService
    {
        private readonly string _contentRootPath;
        private readonly ILogger<LocalFileService> _logger;

        public LocalFileService(string contentRootPath, ILogger<LocalFileService> logger)
        {
            _contentRootPath = contentRootPath;
            _logger = logger;
        }

        public string GetContentRootPath() => _contentRootPath;

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is required.", nameof(file));
            }

            // Folder path set karein (e.g., wwwroot/uploads/resumes)
            string uploadFolder = Path.Combine(_contentRootPath, "wwwroot", "uploads", subFolder);
            
            if (!Directory.Exists(uploadFolder))
            {
                _logger.LogInformation("Creating directory: {UploadFolder}", uploadFolder);
                Directory.CreateDirectory(uploadFolder);
            }

            // Unique file name banayein
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadFolder, fileName);

            _logger.LogInformation("Saving file to: {FilePath}", filePath);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Database mein save karne ke liye relative URL return karein
            string relativeUrl = $"/uploads/{subFolder}/{fileName}";
            _logger.LogInformation("File saved. Relative URL: {RelativeUrl}", relativeUrl);
            return relativeUrl;
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            
            string fullPath = Path.Combine(_contentRootPath, "wwwroot", filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                _logger.LogInformation("Deleting file: {FullPath}", fullPath);
                File.Delete(fullPath);
            }
        }
    }
}
