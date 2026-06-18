using befitapi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace befitapi.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long _maxFileSize = 5 * 1024 * 1024; // 5 MB

        public ImageRepository(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            // 1. التحقق من حجم الملف
            if (file.Length > _maxFileSize)
                throw new Exception("حجم الصورة كبير جداً. الحد الأقصى هو 5 ميجابايت.");

            // 2. التحقق من امتداد الملف
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                throw new Exception("نوع الملف غير مدعوم. المسموح فقط: jpg, jpeg, png, gif, webp");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folderName);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 3. إنشاء اسم فريد وتخزين الملف
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 4. بناء الرابط الكامل
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl}/{folderName}/{uniqueFileName}";
        }

        public void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                // استخراج اسم الملف من الرابط
                var uri = new Uri(imageUrl);
                var fileName = Path.GetFileName(uri.LocalPath);
                var folderName = Path.GetFileName(Path.GetDirectoryName(uri.LocalPath));

                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, folderName, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // فشل الحذف لا يجب أن يعطل العملية الرئيسية
            }
        }

        public string GetImageUrl(string fileName, string folderName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl}/{folderName}/{fileName}";
        }
    }
}
