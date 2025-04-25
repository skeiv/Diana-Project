using System;
using System.Collections.Generic;

namespace JobSearchApp.Core.Entities
{
    // Больше не наследуемся от User, а от BaseEntity
    public class Recruiter : BaseEntity
    {
        // Внешний ключ и навигационное свойство к User
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Свойства, специфичные для Recruiter
        public string? Company { get; set; } // Может работать не в компании?
        public string Specialization { get; set; } = null!;
        public int ExperienceYears { get; set; }
        // Общие свойства пользователя наследуются

        // Navigation Properties
        // Пользователи, которых "ведет" рекрутер?
        // Если да, то нужна явная связь или таблица.
        // Если нет, эта коллекция не нужна.
        // public virtual ICollection<User> ManagedUsers { get; set; } = new HashSet<User>();
        public virtual ICollection<Vacancy> ManagedVacancies { get; set; } = new HashSet<Vacancy>();
    }
} 