using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data
{
    public class LMSDbContext : DbContext
    {
        public LMSDbContext(DbContextOptions<LMSDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<LMS.Domain.Entities.Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<DiscussionForum> DiscussionForums { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region [Ensure Relationships]

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Modules)
                .WithOne(m => m.Course)
                .HasForeignKey(m => m.CourseId);

            modelBuilder.Entity<LMS.Domain.Entities.Module>()
                .HasMany(m => m.Lessons)
                .WithOne(l => l.Module)
                .HasForeignKey(l => l.ModuleId);

            modelBuilder.Entity<Assessment>()
                .HasMany(a => a.Questions)
                .WithOne(q => q.Assessment)
                .HasForeignKey(q => q.AssessmentId);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull); // No cascading delete

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull); // No cascading delete

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.CreatedByUser)
                .WithMany()
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DiscussionForum>()
                .HasMany(df => df.Posts)
                .WithOne(fp => fp.Forum)
                .HasForeignKey(fp => fp.ForumId)
                .OnDelete(DeleteBehavior.ClientSetNull); // No cascading delete

            modelBuilder.Entity<ForumPost>()
               .HasOne(fp => fp.Forum)
               .WithMany(df => df.Posts)
               .HasForeignKey(fp => fp.ForumId)
               .OnDelete(DeleteBehavior.ClientSetNull); // No cascading delete

            #endregion

            #region [Seed Data]    

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "admin", PasswordHash = "AQAAAAIAAYagAAAAEEEQeDbdNV5MgtGE9xRiBmHn6LHimppoTNMc5e+kfJyWOZulKNJ7QFrSr7KxgFsMoA==", Email = "admin@example.com", FullName = "Admin User", DateOfBirth = new DateTime(1980, 1, 1), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = 2, Username = "instructor", PasswordHash = "AQAAAAIAAYagAAAAEEEQeDbdNV5MgtGE9xRiBmHn6LHimppoTNMc5e+kfJyWOZulKNJ7QFrSr7KxgFsMoA==", Email = "instructor@example.com", FullName = "Instructor User", DateOfBirth = new DateTime(1985, 5, 10), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = 3, Username = "student", PasswordHash = "AQAAAAIAAYagAAAAEEEQeDbdNV5MgtGE9xRiBmHn6LHimppoTNMc5e+kfJyWOZulKNJ7QFrSr7KxgFsMoA==", Email = "student@example.com", FullName = "Student User", DateOfBirth = new DateTime(1990, 9, 15), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "Admin", Description = "Administrator with full access" },
                new Role { RoleId = 2, Name = "Instructor", Description = "Instructor with access to create and manage courses" },
                new Role { RoleId = 3, Name = "Student", Description = "Student with access to view and take courses" }
            );

            // Seed UserRoles
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 },
                new UserRole { UserId = 2, RoleId = 2 },
                new UserRole { UserId = 3, RoleId = 3 }
            );

            // Seed Courses
            modelBuilder.Entity<Course>().HasData(
                new Course { CourseId = 1, Title = "Introduction to Programming", Description = "Basic programming concepts", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Course { CourseId = 2, Title = "Advanced .NET Development", Description = ".NET Core and advanced topics", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            // Seed Modules
            modelBuilder.Entity<Module>().HasData(
                new Module { ModuleId = 1, CourseId = 1, Title = "Module 1: Basics", Content = "Introduction to programming basics", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Module { ModuleId = 2, CourseId = 2, Title = "Module 1: Advanced Concepts", Content = "Deep dive into .NET Core", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            // Seed Lessons
            modelBuilder.Entity<Lesson>().HasData(
                new Lesson { LessonId = 1, ModuleId = 1, Title = "Lesson 1: Variables", Content = "Introduction to variables", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Lesson { LessonId = 2, ModuleId = 2, Title = "Lesson 1: Dependency Injection", Content = "Introduction to DI in .NET", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            // Seed Assessments
            modelBuilder.Entity<Assessment>().HasData(
                new Assessment { AssessmentId = 1, CourseId = 1, Title = "Assessment 1: Basics", Description = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Assessment { AssessmentId = 2, CourseId = 2, Title = "Assessment 1: Advanced Concepts", Description = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            // Seed Questions
            modelBuilder.Entity<Question>().HasData(
                new Question { QuestionId = 1, AssessmentId = 1, Content = "What is a variable?", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Question { QuestionId = 2, AssessmentId = 2, Content = "Explain Dependency Injection.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            // Seed Answers
            modelBuilder.Entity<Answer>().HasData(
                new Answer { AnswerId = 1, QuestionId = 1, Content = "A variable is a storage location identified by a memory address.", IsCorrect = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Answer { AnswerId = 2, QuestionId = 2, Content = "DI is a design pattern used to implement IoC, allowing for better decoupling of code.", IsCorrect = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            // Seed Messages
            modelBuilder.Entity<Message>().HasData(
                new Message { MessageId = 1, SenderId = 1, ReceiverId = 2, Content = "Hello Instructor!", SentAt = DateTime.UtcNow },
                new Message { MessageId = 2, SenderId = 2, ReceiverId = 3, Content = "Hello Student!", SentAt = DateTime.UtcNow }
            );

            // Seed Notifications
            modelBuilder.Entity<Notification>().HasData(
                new Notification { NotificationId = 1, UserId = 1, Content = "You have a new message.", CreatedAt = DateTime.UtcNow, IsRead = false },
                new Notification { NotificationId = 2, UserId = 2, Content = "Your course has been updated.", CreatedAt = DateTime.UtcNow, IsRead = false }
            );

            // Seed Announcements
            modelBuilder.Entity<Announcement>().HasData(
                new Announcement { AnnouncementId = 1, Title = "Welcome to the Course", Content = "We are excited to have you in the course!", CreatedAt = DateTime.UtcNow, CreatedBy = 1, CourseId = 1 },
                new Announcement { AnnouncementId = 2, Title = "Course Update", Content = "New materials have been added.", CreatedAt = DateTime.UtcNow, CreatedBy = 2, CourseId = 2 }
            );

            // Seed DiscussionForums
            modelBuilder.Entity<DiscussionForum>().HasData(
                new DiscussionForum { ForumId = 1, Title = "General Discussion", Description = "Discuss anything related to the course.", CreatedAt = DateTime.UtcNow, CreatedBy = 1, CourseId = 1 },
                new DiscussionForum { ForumId = 2, Title = "Technical Questions", Description = "Ask technical questions here.", CreatedAt = DateTime.UtcNow, CreatedBy = 2, CourseId = 2 }
            );

            // Seed ForumPosts
            modelBuilder.Entity<ForumPost>().HasData(
                new ForumPost { PostId = 1, ForumId = 1, Content = "This course is great!", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = 3 },
                new ForumPost { PostId = 2, ForumId = 2, Content = "Can someone explain Dependency Injection?", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = 3 }
            );

            #endregion
        }
    }
}
