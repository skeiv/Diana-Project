using System;
using System.Collections.Generic;

namespace JobSearchApp.Core.Entities
{
    // Больше не наследуемся от User, а от BaseEntity
    public class Teacher : BaseEntity
    {
        // Внешний ключ и навигационное свойство к User
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Свойства, специфичные для Teacher
        public string Specialization { get; set; } = null!;
        public int ExperienceYears { get; set; }
        public string? Education { get; set; }
        public string? Bio { get; set; }

        // Navigation Properties
        public virtual ICollection<Course> Courses { get; set; } = new HashSet<Course>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new HashSet<Assignment>();
    }
} 