using System;
using System.Collections.Generic;

namespace JobSearchApp.Core.Entities
{
    public class Resume : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Skills { get; set; } = null!;
        public string Experience { get; set; } = null!;
        public string Education { get; set; } = null!;
        public string? FileUrl { get; set; }
        public bool IsActive { get; set; }
    }
} 