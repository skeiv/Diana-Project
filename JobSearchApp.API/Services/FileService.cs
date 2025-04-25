using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace JobSearchApp.API.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private const string ResumeFilesFolder = "resumes";

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadResumeFileAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Файл не был предоставлен");

            // Создаем папку для резюме, если она не существует
            var uploadsFolder = Path.Combine(_environment.WebRootPath, ResumeFilesFolder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Генерируем уникальное имя файла
            var fileName = $"{userId}_{DateTime.UtcNow.Ticks}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Сохраняем файл
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Возвращаем относительный путь к файлу
            return $"/{ResumeFilesFolder}/{fileName}";
        }

        public async Task DeleteResumeFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return;

            var filePath = Path.Combine(_environment.WebRootPath, fileUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }

        public async Task<byte[]> GetResumeFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                throw new ArgumentException("URL файла не был предоставлен");

            var filePath = Path.Combine(_environment.WebRootPath, fileUrl.TrimStart('/'));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден");

            return await File.ReadAllBytesAsync(filePath);
        }
    }
} 