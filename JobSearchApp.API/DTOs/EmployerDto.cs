using System.Collections.Generic;

namespace JobSearchApp.API.DTOs
{
    public class EmployerDto : UserDto
    {
        public string CompanyName { get; set; } = null!;
        public string? CompanyDescription { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? WorkExperience { get; set; } = null!;
        public string? Schedule { get; set; } = null!;
        public string? HealthRestrictions { get; set; }
        public string? Resume { get; set; }
    }

    public class EmployerDetailDto : EmployerDto
    {
        public ICollection<VacancyDto> Vacancies { get; set; } = new List<VacancyDto>();
        public ICollection<CourseDto> Courses { get; set; } = new List<CourseDto>();
        public ICollection<CourseDto> CompletedCourses { get; set; } = new List<CourseDto>();
        public ICollection<CourseDto> SavedCourses { get; set; } = new List<CourseDto>();
        public ICollection<VacancyDto> SavedVacancies { get; set; } = new List<VacancyDto>();
    }
} 