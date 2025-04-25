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
    public class CoursesController : ControllerBase
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<UserCourse> _userCourseRepository;

        public CoursesController(
            IRepository<Course> courseRepository,
            IRepository<UserCourse> userCourseRepository)
        {
            _courseRepository = courseRepository;
            _userCourseRepository = userCourseRepository;
        }

        [HttpPost]
        [AuthorizeRoles(UserRole.Admin, UserRole.Teacher)]
        public async Task<IActionResult> CreateCourse([FromBody] Course course)
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            course.TeacherId = teacherId;
            course.IsActive = true;

            await _courseRepository.AddAsync(course);
            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Teacher)]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] Course course)
        {
            if (id != course.Id)
                return BadRequest();

            var existingCourse = await _courseRepository.GetByIdAsync(id);
            if (existingCourse == null)
                return NotFound();

            // Проверяем права доступа
            if (existingCourse.TeacherId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value) && 
                !User.IsInRole(UserRole.Admin.ToString()))
                return Forbid();

            await _courseRepository.UpdateAsync(course);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
                return NotFound();

            return course;
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles(UserRole.Admin)]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
                return NotFound();

            course.IsDeleted = true;
            await _courseRepository.UpdateAsync(course);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> SearchCourses([FromQuery] string query)
        {
            var courses = await _courseRepository.FindAsync(c => 
                !c.IsDeleted && 
                (c.Title.Contains(query) || 
                c.Description.Contains(query)));
            return Ok(courses);
        }

        [HttpPost("{id}/enroll")]
        public async Task<IActionResult> EnrollInCourse(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userCourse = new UserCourse
            {
                UserId = userId,
                CourseId = id,
                IsCompleted = false,
                IsSaved = true
            };

            await _userCourseRepository.AddAsync(userCourse);
            return Ok();
        }

        [HttpGet("enrolled")]
        public async Task<ActionResult<IEnumerable<Course>>> GetEnrolledCourses()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var enrolledCourses = await _userCourseRepository.FindAsync(uc => 
                uc.UserId == userId && 
                uc.IsSaved);
            return Ok(enrolledCourses);
        }

        [HttpGet("teaching")]
        [AuthorizeRoles(UserRole.Teacher)]
        public async Task<ActionResult<IEnumerable<Course>>> GetTeachingCourses()
        {
            var teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var courses = await _courseRepository.FindAsync(c => c.TeacherId == teacherId);
            return Ok(courses);
        }
    }
} 