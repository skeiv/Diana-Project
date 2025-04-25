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
    public class EmployersController : ControllerBase
    {
        private readonly IRepository<Employer> _employerRepository;
        private readonly IRepository<Vacancy> _vacancyRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<UserCourse> _userCourseRepository;
        private readonly IRepository<UserVacancy> _userVacancyRepository;

        public EmployersController(
            IRepository<Employer> employerRepository,
            IRepository<Vacancy> vacancyRepository,
            IRepository<Course> courseRepository,
            IRepository<UserCourse> userCourseRepository,
            IRepository<UserVacancy> userVacancyRepository)
        {
            _employerRepository = employerRepository;
            _vacancyRepository = vacancyRepository;
            _courseRepository = courseRepository;
            _userCourseRepository = userCourseRepository;
            _userVacancyRepository = userVacancyRepository;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateEmployer([FromBody] Employer employer)
        {
            await _employerRepository.AddAsync(employer);
            return CreatedAtAction(nameof(GetEmployer), new { id = employer.Id }, employer);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employer>> GetEmployer(int id)
        {
            var employer = await _employerRepository.GetByIdAsync(id);
            if (employer == null)
                return NotFound();

            // Проверяем права доступа
            if (id != int.Parse(User.Identity.Name) && !User.IsInRole(UserRole.Admin.ToString()))
                return Forbid();

            return employer;
        }

        [HttpPost("{id}/vacancies")]
        public async Task<IActionResult> CreateVacancy(int id, [FromBody] Vacancy vacancy)
        {
            vacancy.EmployerId = id;
            await _vacancyRepository.AddAsync(vacancy);
            return CreatedAtAction(nameof(GetVacancy), new { employerId = id, vacancyId = vacancy.Id }, vacancy);
        }

        [HttpPut("{employerId}/vacancies/{vacancyId}")]
        public async Task<IActionResult> UpdateVacancy(int employerId, int vacancyId, [FromBody] Vacancy vacancy)
        {
            if (vacancyId != vacancy.Id || employerId != vacancy.EmployerId)
                return BadRequest();

            var existingVacancy = await _vacancyRepository.GetByIdAsync(vacancyId);
            if (existingVacancy == null)
                return NotFound();

            await _vacancyRepository.UpdateAsync(vacancy);
            return NoContent();
        }

        [HttpGet("{employerId}/vacancies/{vacancyId}")]
        public async Task<ActionResult<Vacancy>> GetVacancy(int employerId, int vacancyId)
        {
            var vacancy = await _vacancyRepository.GetByIdAsync(vacancyId);
            if (vacancy == null || vacancy.EmployerId != employerId)
                return NotFound();

            return vacancy;
        }

        [HttpPost("{id}/courses")]
        public async Task<IActionResult> CreateCourse(int id, [FromBody] Course course)
        {
            await _courseRepository.AddAsync(course);
            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        [HttpGet("{id}/courses/{courseId}")]
        public async Task<ActionResult<Course>> GetCourse(int id, int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                return NotFound();

            return course;
        }

        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetUsers(int id)
        {
            // Здесь будет логика получения списка пользователей
            return Ok();
        }

        [HttpGet("{employerId}/users/{userId}")]
        public async Task<IActionResult> GetUserDetails(int employerId, int userId)
        {
            // Здесь будет логика получения детальной информации о пользователе
            return Ok();
        }

        [HttpPost("{id}/courses/save")]
        public async Task<IActionResult> SaveCourse(int id, [FromBody] int courseId)
        {
            if (id != int.Parse(User.Identity.Name))
                return Forbid();

            var userCourse = new UserCourse
            {
                UserId = id,
                CourseId = courseId,
                IsSaved = true
            };

            await _userCourseRepository.AddAsync(userCourse);
            return Ok();
        }

        [HttpPost("{id}/vacancies/save")]
        public async Task<IActionResult> SaveVacancy(int id, [FromBody] int vacancyId)
        {
            if (id != int.Parse(User.Identity.Name))
                return Forbid();

            var userVacancy = new UserVacancy
            {
                UserId = id,
                VacancyId = vacancyId,
                IsSaved = true
            };

            await _userVacancyRepository.AddAsync(userVacancy);
            return Ok();
        }

        [HttpGet("{id}/courses")]
        public async Task<ActionResult<IEnumerable<Course>>> GetEmployerCourses(int id)
        {
            if (id != int.Parse(User.Identity.Name) && !User.IsInRole(UserRole.Admin.ToString()))
                return Forbid();

            var userCourses = await _userCourseRepository.FindAsync(uc => uc.UserId == id);
            return Ok(userCourses);
        }

        [HttpGet("{id}/vacancies")]
        public async Task<ActionResult<IEnumerable<Vacancy>>> GetEmployerVacancies(int id)
        {
            if (id != int.Parse(User.Identity.Name) && !User.IsInRole(UserRole.Admin.ToString()))
                return Forbid();

            var userVacancies = await _userVacancyRepository.FindAsync(uv => uv.UserId == id);
            return Ok(userVacancies);
        }
    }
} 