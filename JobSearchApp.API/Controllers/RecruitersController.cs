using JobSearchApp.Core.Authorization;
using JobSearchApp.Core.Entities;
using JobSearchApp.Core.Enums;
using JobSearchApp.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobSearchApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecruitersController : ControllerBase
    {
        private readonly IRepository<Recruiter> _recruiterRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Vacancy> _vacancyRepository;

        public RecruitersController(
            IRepository<Recruiter> recruiterRepository,
            IRepository<User> userRepository,
            IRepository<Vacancy> vacancyRepository)
        {
            _recruiterRepository = recruiterRepository;
            _userRepository = userRepository;
            _vacancyRepository = vacancyRepository;
        }

        [HttpPost]
        [AuthorizeRoles(UserRole.Admin)]
        public async Task<IActionResult> CreateRecruiter([FromBody] Recruiter recruiter)
        {
            if (recruiter.User != null)
            {
                recruiter.User.Role = UserRole.Recruiter;
            }
            
            await _recruiterRepository.AddAsync(recruiter);
            return CreatedAtAction(nameof(GetRecruiter), new { id = recruiter.Id }, recruiter);
        }

        [HttpPut("{id}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Recruiter)]
        public async Task<IActionResult> UpdateRecruiter(int id, [FromBody] Recruiter recruiter)
        {
            if (id != recruiter.Id)
                return BadRequest();

            var existingRecruiter = await _recruiterRepository.GetByIdAsync(id);
            if (existingRecruiter == null)
                return NotFound();

            if (existingRecruiter.UserId != int.Parse(User.Identity.Name) && !User.IsInRole(UserRole.Admin.ToString()))
                return Forbid();

            await _recruiterRepository.UpdateAsync(recruiter);
            return NoContent();
        }

        [HttpGet("{id}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Recruiter)]
        public async Task<ActionResult<Recruiter>> GetRecruiter(int id)
        {
            var recruiter = await _recruiterRepository.GetByIdAsync(id);
            if (recruiter == null)
                return NotFound();

            if (recruiter.UserId != int.Parse(User.Identity.Name) && !User.IsInRole(UserRole.Admin.ToString()))
                return Forbid();

            return recruiter;
        }

        [HttpPost("resumes/send")]
        [AuthorizeRoles(UserRole.Recruiter)]
        public async Task<IActionResult> SendResume([FromBody] ResumeRequest request)
        {
            var recruiter = await _recruiterRepository.GetByIdAsync(request.RecruiterId);
            if (recruiter == null)
                return NotFound("Recruiter not found");

            if (recruiter.UserId != int.Parse(User.Identity.Name))
                return Forbid();

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                return NotFound("User not found");

            // Здесь будет логика отправки резюме
            return Ok();
        }

        [HttpPost("courses/send")]
        [AuthorizeRoles(UserRole.Recruiter)]
        public async Task<IActionResult> SendCourse([FromBody] CourseRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                return NotFound("User not found");

            // Проверяем права доступа
            if (!User.IsInRole(UserRole.Recruiter.ToString()))
                return Forbid();

            // Здесь будет логика отправки курса
            return Ok();
        }

        [HttpGet("managed-vacancies")]
        [AuthorizeRoles(UserRole.Recruiter)]
        public async Task<ActionResult<IEnumerable<Vacancy>>> GetManagedVacancies()
        {
            var recruiterId = int.Parse(User.Identity.Name);
            var vacancies = await _vacancyRepository.FindAsync(v => v.RecruiterId == recruiterId);
            return Ok(vacancies);
        }

        [HttpGet("managed-users")]
        [AuthorizeRoles(UserRole.Recruiter)]
        public async Task<ActionResult<IEnumerable<User>>> GetManagedUsers()
        {
            // Здесь будет логика получения управляемых пользователей
            return Ok(new List<User>());
        }
    }

    public class ResumeRequest
    {
        public int UserId { get; set; }
        public int RecruiterId { get; set; }
    }

    public class CourseRequest
    {
        public int CourseId { get; set; }
        public int UserId { get; set; }
    }
} 