using Microsoft.AspNetCore.Http;

namespace JobSearchApp.API.Services
{
    public interface IFileService
    {
        Task<string> UploadResumeFileAsync(IFormFile file, int userId);
        Task DeleteResumeFileAsync(string fileUrl);
        Task<byte[]> GetResumeFileAsync(string fileUrl);
    }
} 