namespace JobSearchApp.Core.Entities
{
    public class UserCourse : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public bool IsSaved { get; set; }
    }
} 