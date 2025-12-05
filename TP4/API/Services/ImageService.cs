namespace API.Services
{
    public interface IImageService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folder);
        bool DeleteImage(string imagePath);
    }

    public class ImageService : IImageService
    {
        private readonly string _webRootPath;
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageService(IWebHostEnvironment env)
        {
            // Ensure we have a valid wwwroot path even if none is configured
            var root = env?.WebRootPath;
            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            _webRootPath = root;

            // Guarantee the root exists to avoid ArgumentNullException/Directory issues
            Directory.CreateDirectory(_webRootPath);
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            // Folder fallback
            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = "uploads";
            }

            // Validate file size
            if (file.Length > MaxFileSize)
                throw new ArgumentException("File size exceeds 5MB limit");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only JPG, PNG, and GIF are allowed");

            try
            {
                var uploadsFolder = Path.Combine(_webRootPath, "uploads", folder);

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path for URL
                return $"/uploads/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new Exception("Error uploading file", ex);
            }
        }

        public bool DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            try
            {
                var fullPath = Path.Combine(_webRootPath, imagePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
