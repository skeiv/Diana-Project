using System.Collections.Generic;

namespace JobSearchApp.API.DTOs
{
    public class TeacherDto : UserDto
    {
        public string Specialization { get; set; } = null!;
        public int ExperienceYears { get; set; }
        public string? Education { get; set; }
        public string? Bio { get; set; }
    }

    public class TeacherDetailDto : TeacherDto
    {
        public ICollection<CourseDto> Courses { get; set; } = new List<CourseDto>();
        public ICollection<AssignmentDto> Assignments { get; set; } = new List<AssignmentDto>();
    }
} 