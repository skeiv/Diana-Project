using JobSearchApp.Core.Entities;
using JobSearchApp.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using JobSearchApp.Core.Authorization;
using JobSearchApp.Core.Enums;

namespace JobSearchApp.API.Controllers
{
    [AuthorizeRoles(UserRole.Teacher)]
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly IRepository<Teacher> _teacherRepository;
        private readonly IRepository<Course> _courseRepository;

        public TeachersController(
            IRepository<Teacher> teacherRepository,
            IRepository<Course> courseRepository)
        {
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
        }

        [HttpGet("{id}/courses")]
        public async Task<ActionResult<IEnumerable<Course>>> GetTeacherCourses(int id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
                return NotFound();

            return Ok(teacher.Courses);
        }

        [HttpGet("courses/{courseId}/students")]
        public async Task<IActionResult> GetCourseStudents(int courseId)
        {
            // Здесь будет логика получения списка студентов курса
            return Ok();
        }

        [HttpGet("{id}/courses/{courseId}/assignments-results")]
        public async Task<IActionResult> GetAssignmentsResults(int id, int courseId)
        {
            // Здесь будет логика получения результатов заданий
            return Ok();
        }

        [HttpPost("{id}/assignments/grade")]
        public async Task<IActionResult> GradeAssignment(int id, [FromBody] GradeRequest request)
        {
            // Здесь будет логика оценки задания
            return Ok();
        }

        [HttpPost("{id}/students/{studentId}/feedback")]
        public async Task<IActionResult> ProvideFeedback(int id, int studentId, [FromBody] FeedbackRequest request)
        {
            // Здесь будет логика предоставления обратной связи
            return Ok();
        }

        public class GradeRequest
        {
            public int StudentId { get; set; }
            public int AssignmentId { get; set; }
            public string Grade { get; set; }
            public string Comment { get; set; }
        }

        public class FeedbackRequest
        {
            public string Message { get; set; }
        }

        public class GradeAssignmentModel
        {
            public int Grade { get; set; }
            public string? Comment { get; set; }
        }

        public class SendMessageModel
        {
            public string Message { get; set; } = null!;
        }
    }
} 