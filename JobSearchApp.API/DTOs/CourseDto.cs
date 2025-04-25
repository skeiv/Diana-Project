using System.Collections.Generic;

namespace JobSearchApp.API.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Format { get; set; }
        public int Hours { get; set; }
        public string? FinalProfession { get; set; }
        public decimal Price { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudents { get; set; }
        public bool IsActive { get; set; }
        public EmployerDto? Employer { get; set; }
    }

    public class CourseDetailDto : CourseDto
    {
        public ICollection<TeacherDto> Teachers { get; set; } = new List<TeacherDto>();
        public ICollection<AssignmentDto> Assignments { get; set; } = new List<AssignmentDto>();
    }
} 