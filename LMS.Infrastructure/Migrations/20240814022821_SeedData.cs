using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "CourseId", "CreatedAt", "Description", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2329), "Basic programming concepts", "Introduction to Programming", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2333) },
                    { 2, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2337), ".NET Core and advanced topics", "Advanced .NET Development", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2338) }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Administrator with full access", "Admin" },
                    { 2, "Instructor with access to create and manage courses", "Instructor" },
                    { 3, "Student with access to view and take courses", "Student" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "DateOfBirth", "Email", "FullName", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(1563), new DateTime(1980, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@example.com", "Admin User", "AQAAAAIAAYagAAAAEEEQeDbdNV5MgtGE9xRiBmHn6LHimppoTNMc5e+kfJyWOZulKNJ7QFrSr7KxgFsMoA==", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(1564), "admin" },
                    { 2, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(1570), new DateTime(1985, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "instructor@example.com", "Instructor User", "AQAAAAIAAYagAAAAEEEQeDbdNV5MgtGE9xRiBmHn6LHimppoTNMc5e+kfJyWOZulKNJ7QFrSr7KxgFsMoA==", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(1571), "instructor" },
                    { 3, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(1575), new DateTime(1990, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "student@example.com", "Student User", "AQAAAAIAAYagAAAAEEEQeDbdNV5MgtGE9xRiBmHn6LHimppoTNMc5e+kfJyWOZulKNJ7QFrSr7KxgFsMoA==", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(1576), "student" }
                });

            migrationBuilder.InsertData(
                table: "Announcements",
                columns: new[] { "AnnouncementId", "Content", "CourseId", "CreatedAt", "CreatedBy", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "We are excited to have you in the course!", 1, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2854), 1, "Welcome to the Course", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2852) },
                    { 2, "New materials have been added.", 2, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2858), 2, "Course Update", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2857) }
                });

            migrationBuilder.InsertData(
                table: "Assessments",
                columns: new[] { "AssessmentId", "CourseId", "CreatedAt", "Description", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2543), "", "Assessment 1: Basics", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2545) },
                    { 2, 2, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2549), "", "Assessment 1: Advanced Concepts", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2549) }
                });

            migrationBuilder.InsertData(
                table: "DiscussionForums",
                columns: new[] { "ForumId", "CourseId", "CreatedAt", "CreatedBy", "Description", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2909), 1, "Discuss anything related to the course.", "General Discussion", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2905) },
                    { 2, 2, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2913), 2, "Ask technical questions here.", "Technical Questions", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2912) }
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "MessageId", "Content", "ReceiverId", "SenderId", "SentAt" },
                values: new object[,]
                {
                    { 1, "Hello Instructor!", 2, 1, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2733) },
                    { 2, "Hello Student!", 3, 2, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2737) }
                });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "ModuleId", "Content", "CourseId", "CreatedAt", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Introduction to programming basics", 1, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2400), "Module 1: Basics", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2402) },
                    { 2, "Deep dive into .NET Core", 2, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2406), "Module 1: Advanced Concepts", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2406) }
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "NotificationId", "Content", "CreatedAt", "IsRead", "UserId" },
                values: new object[,]
                {
                    { 1, "You have a new message.", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2802), false, 1 },
                    { 2, "Your course has been updated.", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2807), false, 2 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 }
                });

            migrationBuilder.InsertData(
                table: "ForumPosts",
                columns: new[] { "PostId", "Content", "CreatedAt", "CreatedBy", "ForumId", "ParentPostId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "This course is great!", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(3051), 3, 1, null, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(3053) },
                    { 2, "Can someone explain Dependency Injection?", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(3058), 3, 2, null, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(3058) }
                });

            migrationBuilder.InsertData(
                table: "Lessons",
                columns: new[] { "LessonId", "Content", "CreatedAt", "ModuleId", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Introduction to variables", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2480), 1, "Lesson 1: Variables", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2480) },
                    { 2, "Introduction to DI in .NET", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2484), 2, "Lesson 1: Dependency Injection", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2485) }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "QuestionId", "AssessmentId", "Content", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "What is a variable?", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2608), new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2610) },
                    { 2, 2, "Explain Dependency Injection.", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2614), new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2615) }
                });

            migrationBuilder.InsertData(
                table: "Answers",
                columns: new[] { "AnswerId", "Content", "CreatedAt", "IsCorrect", "QuestionId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "A variable is a storage location identified by a memory address.", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2685), true, 1, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2686) },
                    { 2, "DI is a design pattern used to implement IoC, allowing for better decoupling of code.", new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2690), true, 2, new DateTime(2024, 8, 14, 2, 28, 20, 53, DateTimeKind.Utc).AddTicks(2691) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Announcements",
                keyColumn: "AnnouncementId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Announcements",
                keyColumn: "AnnouncementId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Answers",
                keyColumn: "AnswerId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Answers",
                keyColumn: "AnswerId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ForumPosts",
                keyColumn: "PostId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ForumPosts",
                keyColumn: "PostId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Lessons",
                keyColumn: "LessonId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Lessons",
                keyColumn: "LessonId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Notifications",
                keyColumn: "NotificationId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Notifications",
                keyColumn: "NotificationId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 3, 3 });

            migrationBuilder.DeleteData(
                table: "DiscussionForums",
                keyColumn: "ForumId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DiscussionForums",
                keyColumn: "ForumId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "ModuleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "QuestionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "QuestionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Assessments",
                keyColumn: "AssessmentId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Assessments",
                keyColumn: "AssessmentId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "CourseId",
                keyValue: 2);
        }
    }
}
