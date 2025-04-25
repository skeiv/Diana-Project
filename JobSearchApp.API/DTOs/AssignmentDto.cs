using System;
using System.Collections.Generic;
using JobSearchApp.API.DTOs;

namespace JobSearchApp.API.DTOs
{
    public class AssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Grade { get; set; }
        public string? Feedback { get; set; }
        public CourseDto? Course { get; set; }
        public TeacherDto? Teacher { get; set; }
        public UserDto? Student { get; set; }
    }
} 