using JobSearchApp.Core.Authorization;
using JobSearchApp.Core.Entities;
using JobSearchApp.Core.Enums;
using JobSearchApp.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobSearchApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VacanciesController : ControllerBase
    {
        private readonly IRepository<Vacancy> _vacancyRepository;
        private readonly IRepository<Recruiter> _recruiterRepository;
        private readonly IRepository<VacancyApplication> _applicationRepository;

        public VacanciesController(
            IRepository<Vacancy> vacancyRepository,
            IRepository<Recruiter> recruiterRepository,
            IRepository<VacancyApplication> applicationRepository)
        {
            _vacancyRepository = vacancyRepository;
            _recruiterRepository = recruiterRepository;
            _applicationRepository = applicationRepository;
        }

        [HttpPost]
        [AuthorizeRoles(UserRole.Recruiter)]
        public async Task<IActionResult> CreateVacancy([FromBody] Vacancy vacancy)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }
            vacancy.RecruiterId = userId;
            vacancy.IsActive = true;

            await _vacancyRepository.AddAsync(vacancy);
            return CreatedAtAction(nameof(GetVacancy), new { id = vacancy.Id }, vacancy);
        }

        [HttpPut("{id}")]
        [AuthorizeRoles(UserRole.Recruiter)]
        public async Task<IActionResult> UpdateVacancy(int id, [FromBody] Vacancy vacancy)
        {
            if (id != vacancy.Id)
                return BadRequest();

            var existingVacancy = await _vacancyRepository.GetByIdAsync(id);
            if (existingVacancy == null)
                return NotFound();

            // Проверяем права доступа
            if (existingVacancy.RecruiterId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value))
                return Forbid();

            await _vacancyRepository.UpdateAsync(vacancy);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Vacancy>> GetVacancy(int id)
        {
            var vacancy = await _vacancyRepository.GetByIdAsync(id);
            if (vacancy == null)
                return NotFound();

            return vacancy;
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles(UserRole.Recruiter)]
        public async Task<IActionResult> DeleteVacancy(int id)
        {
            var vacancy = await _vacancyRepository.GetByIdAsync(id);
            if (vacancy == null)
                return NotFound();

            // Проверяем права доступа
            if (vacancy.RecruiterId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value))
                return Forbid();

            vacancy.IsActive = false;
            await _vacancyRepository.UpdateAsync(vacancy);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vacancy>>> SearchVacancies([FromQuery] string query)
        {
            var vacancies = await _vacancyRepository.FindAsync(v => 
                v.IsActive && 
                (v.Title.Contains(query) || 
                v.Description.Contains(query)));
            return Ok(vacancies);
        }

        [HttpPost("{id}/apply")]
        [AuthorizeRoles(UserRole.User)]
        public async Task<IActionResult> ApplyForVacancy(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Не удалось определить пользователя.");
            }
            var application = new VacancyApplication
            {
                VacancyId = id,
                UserId = userId,
                Status = ApplicationStatus.Pending
            };

            await _applicationRepository.AddAsync(application);
            return Ok();
        }

        [HttpGet("applications")]
        [AuthorizeRoles(UserRole.Recruiter)]
        public async Task<ActionResult<IEnumerable<VacancyApplication>>> GetVacancyApplications()
        {
            var recruiterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var vacancies = await _vacancyRepository.FindAsync(v => v.RecruiterId == recruiterId);
            var vacancyIds = vacancies.Select(v => v.Id);
            
            var applications = await _applicationRepository.FindAsync(a => 
                vacancyIds.Contains(a.VacancyId));
            return Ok(applications);
        }

        [HttpPut("applications/{id}/status")]
        [AuthorizeRoles(UserRole.Recruiter)]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] ApplicationStatus status)
        {
            var application = await _applicationRepository.GetByIdAsync(id);
            if (application == null)
                return NotFound();

            var vacancy = await _vacancyRepository.GetByIdAsync(application.VacancyId);
            if (vacancy.RecruiterId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value))
                return Forbid();

            application.Status = status;
            await _applicationRepository.UpdateAsync(application);
            return NoContent();
        }
    }
} 