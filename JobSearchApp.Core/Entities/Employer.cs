using System;
using System.Collections.Generic;

namespace JobSearchApp.Core.Entities
{

    public class Employer : User
    {
        public string CompanyName { get; set; } = null!;
        public string? CompanyDescription { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        // Общие свойства пользователя (Email, Name и т.д.) наследуются от User

        // Убираем дублирующие/ненужные свойства, если они есть в User
        // public string? WorkExperience { get; set; }
        // public string? Schedule { get; set; }
        // public string? HealthRestrictions { get; set; }
        // public string? Resume { get; set; } // Это свойство кажется неуместным для Employer

        // Navigation Properties
        public virtual ICollection<Vacancy> Vacancies { get; set; } = new HashSet<Vacancy>();
        public virtual ICollection<Course> Courses { get; set; } = new HashSet<Course>();
        // Следующие коллекции, вероятно, не нужны напрямую в Employer,
        // так как UserCourse и UserVacancy - это отдельные сущности связи.
        // Оставляю их закомментированными на случай, если они нужны для чего-то специфичного.
        // public virtual ICollection<UserCourse> UserCourses { get; set; } = new HashSet<UserCourse>();
        // public virtual ICollection<UserVacancy> UserVacancies { get; set; } = new HashSet<UserVacancy>();
    }
} 