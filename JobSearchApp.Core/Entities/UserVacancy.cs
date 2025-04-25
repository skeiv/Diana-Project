namespace JobSearchApp.Core.Entities
{
    public class UserVacancy : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public int VacancyId { get; set; }
        public virtual Vacancy Vacancy { get; set; } = null!;
        public bool IsSaved { get; set; }
        public bool IsCurrent { get; set; }
    }
} 