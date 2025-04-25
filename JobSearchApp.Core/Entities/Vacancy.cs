using System.Collections.Generic;

namespace JobSearchApp.Core.Entities
{
    public class Vacancy : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<string> Requirements { get; set; } = new List<string>();
        public string Salary { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string WorkingConditions { get; set; } = null!;
        public string Rate { get; set; } = null!;
        public bool IsActive { get; set; }

        // Навигационные свойства
        public virtual Employer Employer { get; set; } = null!;
        public int EmployerId { get; set; }
        public virtual Recruiter? Recruiter { get; set; }
        public int? RecruiterId { get; set; }
    }
} 