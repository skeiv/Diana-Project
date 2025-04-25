using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobSearchApp.Infrastructure.Data;
using JobSearchApp.API.DTOs;
using JobSearchApp.Core.Entities;
using JobSearchApp.API.Services;
using System.Security.Claims;

namespace JobSearchApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResumesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IFileService _fileService;

        public ResumesController(
            ApplicationDbContext context, 
            IUserService userService,
            IFileService fileService)
        {
            _context = context;
            _userService = userService;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResumeDto>>> GetResumes()
        {
            var currentUser = await _userService.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var resumes = await _context.Resumes
                .Where(r => r.UserId == currentUser.Id)
                .Select(r => new ResumeDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Title = r.Title,
                    Description = r.Description,
                    Skills = r.Skills,
                    Experience = r.Experience,
                    Education = r.Education,
                    FileUrl = r.FileUrl,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return Ok(resumes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResumeDto>> GetResume(int id)
        {
            var currentUser = await _userService.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (resume == null)
                return NotFound();

            return new ResumeDto
            {
                Id = resume.Id,
                UserId = resume.UserId,
                Title = resume.Title,
                Description = resume.Description,
                Skills = resume.Skills,
                Experience = resume.Experience,
                Education = resume.Education,
                FileUrl = resume.FileUrl,
                IsActive = resume.IsActive,
                CreatedAt = resume.CreatedAt,
                UpdatedAt = resume.UpdatedAt
            };
        }

        [HttpPost]
        public async Task<ActionResult<ResumeDto>> CreateResume(CreateResumeDto createResumeDto)
        {
            var currentUser = await _userService.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var resume = new Resume
            {
                UserId = currentUser.Id,
                Title = createResumeDto.Title,
                Description = createResumeDto.Description,
                Skills = createResumeDto.Skills,
                Experience = createResumeDto.Experience,
                Education = createResumeDto.Education,
                FileUrl = createResumeDto.FileUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResume), new { id = resume.Id }, new ResumeDto
            {
                Id = resume.Id,
                UserId = resume.UserId,
                Title = resume.Title,
                Description = resume.Description,
                Skills = resume.Skills,
                Experience = resume.Experience,
                Education = resume.Education,
                FileUrl = resume.FileUrl,
                IsActive = resume.IsActive,
                CreatedAt = resume.CreatedAt,
                UpdatedAt = resume.UpdatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResume(int id, UpdateResumeDto updateResumeDto)
        {
            var currentUser = await _userService.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (resume == null)
                return NotFound();

            resume.Title = updateResumeDto.Title;
            resume.Description = updateResumeDto.Description;
            resume.Skills = updateResumeDto.Skills;
            resume.Experience = updateResumeDto.Experience;
            resume.Education = updateResumeDto.Education;
            resume.FileUrl = updateResumeDto.FileUrl;
            resume.IsActive = updateResumeDto.IsActive;
            resume.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResume(int id)
        {
            var currentUser = await _userService.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (resume == null)
                return NotFound();

            // Удаляем файл резюме, если он существует
            if (!string.IsNullOrEmpty(resume.FileUrl))
            {
                await _fileService.DeleteResumeFileAsync(resume.FileUrl);
            }

            _context.Resumes.Remove(resume);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/file")]
        public async Task<ActionResult<string>> UploadResumeFile(int id, IFormFile file)
        {
            var currentUser = await _userService.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (resume == null)
                return NotFound();

            // Удаляем старый файл, если он существует
            if (!string.IsNullOrEmpty(resume.FileUrl))
            {
                await _fileService.DeleteResumeFileAsync(resume.FileUrl);
            }

            // Загружаем новый файл
            var fileUrl = await _fileService.UploadResumeFileAsync(file, currentUser.Id);
            resume.FileUrl = fileUrl;
            resume.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { fileUrl });
        }

        [HttpGet("{id}/file")]
        public async Task<IActionResult> DownloadResumeFile(int id)
        {
            var currentUser = await _userService.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized();

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (resume == null)
                return NotFound();

            if (string.IsNullOrEmpty(resume.FileUrl))
                return NotFound("Файл резюме не найден");

            try
            {
                var fileBytes = await _fileService.GetResumeFileAsync(resume.FileUrl);
                return File(fileBytes, "application/pdf", $"resume_{id}.pdf");
            }
            catch (FileNotFoundException)
            {
                return NotFound("Файл резюме не найден");
            }
        }
    }
} 