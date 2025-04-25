using System;
using System.Collections.Generic;

namespace JobSearchApp.Core.Entities
{
    public class Course : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Format { get; set; } = null!;
        public int Hours { get; set; }
        public string? FinalProfession { get; set; } // Может быть null
        public decimal Price { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudents { get; set; }
        public bool IsActive { get; set; }
        public int TeacherId { get; set; }

        // Foreign Keys
        public int EmployerId { get; set; }

        // Navigation Properties
        public virtual Employer Employer { get; set; } = null!;
        public virtual ICollection<Teacher> Teachers { get; set; } = new HashSet<Teacher>();
        public virtual ICollection<User> Students { get; set; } = new HashSet<User>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new HashSet<Assignment>();
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
    }
} 