using JobSearchApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Recruiter> Recruiters { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<UserVacancy> UserVacancies { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<VacancyApplication> VacancyApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация User
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired();

            // Удаляем явное указание таблицы для TPH
            // modelBuilder.Entity<Recruiter>().ToTable("Users");
            // modelBuilder.Entity<Teacher>().ToTable("Users");

            // Конфигурация один-к-одному User <-> Recruiter
            modelBuilder.Entity<User>()
                .HasOne(u => u.Recruiter)
                .WithOne(r => r.User)
                .HasForeignKey<Recruiter>(r => r.UserId);

            // Конфигурация один-к-одному User <-> Teacher
            modelBuilder.Entity<User>()
                .HasOne(u => u.Teacher)
                .WithOne(t => t.User)
                .HasForeignKey<Teacher>(t => t.UserId);

            // Конфигурация User <-> Vacancy (Saved Vacancies)
            modelBuilder.Entity<User>()
                .HasMany(u => u.SavedVacancies)
                .WithMany() // Без обратной навигации в Vacancy
                .UsingEntity("UserSavedVacancy"); // Явно указываем имя таблицы связи

            // Конфигурация User <-> Course (Saved Courses)
            modelBuilder.Entity<User>()
                .HasMany(u => u.SavedCourses)
                .WithMany() // Без обратной навигации в Course
                .UsingEntity("UserSavedCourse"); // Явно указываем имя таблицы связи

            // Конфигурация Course
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Employer)
                .WithMany(e => e.Courses)
                .HasForeignKey(c => c.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Teachers)
                .WithMany(t => t.Courses);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Students)
                .WithMany(u => u.CompletedCourses);

            // Конфигурация Vacancy
            modelBuilder.Entity<Vacancy>()
                .HasOne(v => v.Employer)
                .WithMany(e => e.Vacancies)
                .HasForeignKey(v => v.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь Vacancy с Recruiter (уже должна быть корректной, т.к. ForeignKey был указан)
            modelBuilder.Entity<Vacancy>()
                .HasOne(v => v.Recruiter)
                .WithMany(r => r.ManagedVacancies) // Recruiter теперь имеет коллекцию
                .HasForeignKey(v => v.RecruiterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация Assignment
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь Assignment с Teacher (уже должна быть корректной, т.к. ForeignKey был указан)
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Teacher)
                .WithMany(t => t.Assignments) // Teacher теперь имеет коллекцию
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь Assignment со Student (User) - исправляем WithMany()
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Student) // Student - это User
                .WithMany() // У User нет прямой коллекции Assignments, оставляем так или добавляем ее в User
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация UserCourse
            modelBuilder.Entity<UserCourse>()
                .HasOne(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserCourse>()
                .HasOne(uc => uc.Course)
                .WithMany()
                .HasForeignKey(uc => uc.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация UserVacancy
            modelBuilder.Entity<UserVacancy>()
                .HasOne(uv => uv.User)
                .WithMany(u => u.UserVacancies)
                .HasForeignKey(uv => uv.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserVacancy>()
                .HasOne(uv => uv.Vacancy)
                .WithMany()
                .HasForeignKey(uv => uv.VacancyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация Resume
            modelBuilder.Entity<Resume>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация VacancyApplication (пример, может потребовать корректировки)
            modelBuilder.Entity<VacancyApplication>()
                .HasOne(va => va.Resume)
                .WithMany() // У резюме может быть много заявок
                .HasForeignKey(va => va.ResumeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VacancyApplication>()
                .HasOne(va => va.Vacancy)
                .WithMany() // У вакансии может быть много заявок
                .HasForeignKey(va => va.VacancyId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<VacancyApplication>()
                .HasOne(va => va.User)
                .WithMany() // Пользователь может подать много заявок
                .HasForeignKey(va => va.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Глобальный фильтр для мягкого удаления
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Vacancy>().HasQueryFilter(v => !v.IsDeleted);
            modelBuilder.Entity<Assignment>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<UserCourse>().HasQueryFilter(uc => !uc.IsDeleted);
            modelBuilder.Entity<Resume>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<VacancyApplication>().HasQueryFilter(va => !va.IsDeleted);
        }
    }
} 