using System;
using JobSearchApp.Core.Enums;

namespace JobSearchApp.Core.Entities
{
    public class VacancyApplication : BaseEntity
    {
        public int VacancyId { get; set; }
        public virtual Vacancy Vacancy { get; set; } = null!;
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public int ResumeId { get; set; }
        public virtual Resume Resume { get; set; } = null!;
        public ApplicationStatus Status { get; set; }
    }
} 