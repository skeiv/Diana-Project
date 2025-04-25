using System.Collections.Generic;

namespace JobSearchApp.API.DTOs
{
    public class RecruiterDto : UserDto
    {
        public string? Company { get; set; }
        public string Specialization { get; set; } = null!;
        public int ExperienceYears { get; set; }
    }

    public class RecruiterDetailDto : RecruiterDto
    {
        public ICollection<VacancyDto> ManagedVacancies { get; set; } = new List<VacancyDto>();
        public ICollection<UserDto> ManagedUsers { get; set; } = new List<UserDto>();
    }
} 