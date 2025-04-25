using JobSearchApp.Core.Authorization;
using JobSearchApp.Core.Entities;
using JobSearchApp.Core.Enums;
using JobSearchApp.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using JobSearchApp.API.Services;
using JobSearchApp.API.DTOs;
using System.Security.Claims;

namespace JobSearchApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IRepository<Core.Entities.User> _userRepository;
        private readonly IRepository<Resume> _resumeRepository;
        private readonly IRepository<VacancyApplication> _applicationRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserService _userService;
        private readonly IFileService _fileService;

        public UsersController(
            IRepository<Core.Entities.User> userRepository,
            IRepository<Resume> resumeRepository,
            IRepository<VacancyApplication> applicationRepository,
            IWebHostEnvironment environment,
            IUserService userService,
            IFileService fileService)
        {
            _userRepository = userRepository;
            _resumeRepository = resumeRepository;
            _applicationRepository = applicationRepository;
            _environment = environment;
            _userService = userService;
            _fileService = fileService;
        }

        [HttpPost]
        [AuthorizeRoles(UserRole.Admin)]
        public async Task<IActionResult> CreateUser([FromBody] Core.Entities.User user)
        {
            await _userRepository.AddAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.User)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] Core.Entities.User user)
        {
            if (id != user.Id)
                return BadRequest();

            // Проверяем права доступа
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId) || userId != id)
            {
                if (!User.IsInRole(UserRole.Admin.ToString()))
                    return Forbid();
            }

            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            await _userRepository.UpdateAsync(user);
            return NoContent();
        }

        [HttpGet("{id}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.User)]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles(UserRole.Admin)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsDeleted = true;
            await _userRepository.UpdateAsync(user);
            return NoContent();
        }

        [HttpGet("search")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Recruiter)]
        public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsers([FromQuery] string query)
        {
            var users = await _userService.SearchUsersAsync(query);
            return Ok(users);
        }

        [HttpGet("{id}/saved-courses")]
        public async Task<ActionResult<IEnumerable<int>>> GetSavedCourses(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user.SavedCourses);
        }

        [HttpPost("resume")]
        [AuthorizeRoles(UserRole.User)]
        public async Task<IActionResult> CreateResume([FromBody] Resume resume)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }
            resume.UserId = userId;

            await _resumeRepository.AddAsync(resume);
            return Ok(resume);
        }

        [HttpPut("resume/{id}")]
        [AuthorizeRoles(UserRole.User)]
        public async Task<IActionResult> UpdateResume(int id, [FromBody] Resume resume)
        {
            if (id != resume.Id)
                return BadRequest();

            var existingResume = await _resumeRepository.GetByIdAsync(id);
            if (existingResume == null)
                return NotFound();

            // Проверяем права доступа
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId) || existingResume.UserId != userId)
            {
                return Forbid();
            }

            await _resumeRepository.UpdateAsync(resume);
            return NoContent();
        }

        [HttpGet("resume")]
        [AuthorizeRoles(UserRole.User)]
        public async Task<ActionResult<Resume>> GetUserResume()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }
            var resume = await _resumeRepository.FindAsync(r => r.UserId == userId);
            return Ok(resume.FirstOrDefault());
        }

        [HttpGet("applications")]
        [AuthorizeRoles(UserRole.User)]
        public async Task<ActionResult<IEnumerable<VacancyApplication>>> GetUserApplications()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }
            var applications = await _applicationRepository.FindAsync(a => a.UserId == userId);
            return Ok(applications);
        }

        [HttpPost("avatar")]
        [AuthorizeRoles(UserRole.Admin, UserRole.User)]
        public async Task<ActionResult<string>> UploadAvatar(IFormFile file)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }
            var avatarUrl = await _fileService.UploadResumeFileAsync(file, userId);
            return Ok(new { avatarUrl });
        }

        [HttpGet("avatar")]
        [AuthorizeRoles(UserRole.Admin, UserRole.User)]
        public async Task<IActionResult> GetAvatar()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (string.IsNullOrEmpty(user.AvatarUrl))
                return NotFound("No avatar found");

            var filePath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound("Avatar file not found");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "image/jpeg");
        }

        [HttpDelete("avatar")]
        [AuthorizeRoles(UserRole.Admin, UserRole.User)]
        public async Task<ActionResult> DeleteAvatar()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }
            await _fileService.DeleteResumeFileAsync(userId.ToString());
            return NoContent();
        }

        [HttpGet("employers")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetEmployers()
        {
            var employers = await _userService.GetUsersByRoleAsync(UserRole.Employer);
            return Ok(employers);
        }

        [HttpGet("teachers")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetTeachers()
        {
            var teachers = await _userService.GetUsersByRoleAsync(UserRole.Teacher);
            return Ok(teachers);
        }

        [HttpGet("jobseekers")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetJobSeekers()
        {
            var jobSeekers = await _userService.GetUsersByRoleAsync(UserRole.User);
            return Ok(jobSeekers);
        }
    }
} 