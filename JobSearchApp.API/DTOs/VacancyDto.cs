using System.Collections.Generic;

namespace JobSearchApp.API.DTOs
{
    public class VacancyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public List<string> Requirements { get; set; } = new List<string>();
        public string? Salary { get; set; }
        public string? Location { get; set; }
        public string? WorkingConditions { get; set; }
        public string? Rate { get; set; }
        public bool IsActive { get; set; }
        public EmployerDto? Employer { get; set; }
        public RecruiterDto? Recruiter { get; set; }
    }

    public class VacancyDetailDto : VacancyDto
    {
        public ICollection<UserDto> Applicants { get; set; } = new List<UserDto>();
    }
} 