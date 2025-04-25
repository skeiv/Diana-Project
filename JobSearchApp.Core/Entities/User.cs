using System;
using System.Collections.Generic;
using JobSearchApp.Core.Enums;

namespace JobSearchApp.Core.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; }

        // Навигационные свойства
        public virtual ICollection<Course> CompletedCourses { get; set; } = new HashSet<Course>();
        public virtual ICollection<Course> SavedCourses { get; set; } = new HashSet<Course>();
        public virtual ICollection<Vacancy> SavedVacancies { get; set; } = new HashSet<Vacancy>();
        public virtual ICollection<UserVacancy> UserVacancies { get; set; } = new HashSet<UserVacancy>();
        public virtual Vacancy? CurrentVacancy { get; set; }
        public int? CurrentVacancyId { get; set; }

        // Ссылки на связанные профили (если User - это Recruiter или Teacher)
        public virtual Recruiter? Recruiter { get; set; }
        public virtual Teacher? Teacher { get; set; }
    }
} 