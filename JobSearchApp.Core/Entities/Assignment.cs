using System;
using System.Collections.Generic;
using JobSearchApp.Core.Enums;

namespace JobSearchApp.Core.Entities
{
    public class Assignment : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Grade { get; set; } // Оценка может быть null
        public string? Feedback { get; set; } // Отзыв может быть null

        // Foreign Keys
        public int CourseId { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }

        // Navigation Properties
        public virtual Course Course { get; set; } = null!;
        public virtual Teacher Teacher { get; set; } = null!;
        public virtual User Student { get; set; } = null!;
    }
} 