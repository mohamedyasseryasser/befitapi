using Microsoft.AspNetCore.Http;

namespace befitapi.Interfaces
{
    public interface IImageRepository
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
        void DeleteImage(string imageUrl);
        string GetImageUrl(string fileName, string folderName);
    }
}
