using System;
using System.Collections.Generic;
using JobSearchApp.API.DTOs;

namespace JobSearchApp.API.DTOs // Проверяем namespace - должен быть API
{
    public class ResumeDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Skills { get; set; }
        public string? Experience { get; set; }
        public string? Education { get; set; }
        public string? FileUrl { get; set; } // Уже nullable
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } // Уже nullable
    }

    public class CreateResumeDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Skills { get; set; }
        public string? Experience { get; set; }
        public string? Education { get; set; }
        public string? FileUrl { get; set; }
    }

    public class UpdateResumeDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Skills { get; set; }
        public string? Experience { get; set; }
        public string? Education { get; set; }
        public string? FileUrl { get; set; }
        public bool IsActive { get; set; }
    }
} 