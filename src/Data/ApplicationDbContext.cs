using light_quiz_api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace light_quiz_api.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ConfigureQuizzesTable(builder);
            ConfigureQuestionsTable(builder);
            ConfigureQuestionOptionsTable(builder);
            ConfigureQuestionTypesTable(builder);
            ConfigureQuizAttemptsTable(builder);
            ConfigureUserResultsTable(builder);
            ConfigureStudentQuizSubmissionsTable(builder);
            ConfigureStudentAnswersTable(builder);
            ConfigureGroupsTable(builder);
            ConfigureGroupMembersTable(builder);
            ConfigureInvitationsTable(builder);

            base.OnModelCreating(builder);
        }

        #region Tables
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<QuestionType> QuestionTypes { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<UserResult> UserResults { get; set; }
        public DbSet<StudentQuizSubmission> StudentQuizSubmissions { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        #endregion

        private void ConfigureQuizzesTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<Quiz>();

            // Table name
            builder.ToTable("quizzes");

            // Primary key
            builder.HasKey(q => q.Id);

            builder.HasIndex(q => q.ShortCode)
                .IsUnique()
                .HasDatabaseName("IX_Quizzes_ShortCode");

            // Properties
            builder.Property(q => q.Title).IsRequired().HasMaxLength(255);
            builder.Property(q => q.Description).IsRequired().HasMaxLength(255);
            builder.Property(q => q.StartsAt).IsRequired();
            builder.Property(q => q.DurationMinutes).IsRequired();
            builder.Property(q => q.CreatedBy).IsRequired();
            builder.Property(q => q.CreatedAt).IsRequired();
            builder.Property(q => q.Anonymous).IsRequired();

            // Relationships
            builder.HasOne(q => q.Group)
                .WithMany(g => g.Quizzes)
                .HasForeignKey(q => q.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(q => q.CreatedByUser)
                .WithMany(u => u.CreatedQuizzes)
                .HasForeignKey(q => q.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureQuestionsTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<Question>();

            // Table name
            builder.ToTable("questions");

            // Primary key
            builder.HasKey(q => q.Id);

            // Properties
            builder.Property(q => q.QuizId).IsRequired();
            builder.Property(q => q.QuestionText).IsRequired();
            builder.Property(q => q.QuestionTypeId).IsRequired();
            builder.Property(q => q.Points).IsRequired();
            builder.Property(q => q.CorrectAnswer).IsRequired();

            // Relationships
            builder.HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId);

            builder.HasOne(q => q.QuestionType)
                .WithMany(qt => qt.Questions)
                .HasForeignKey(q => q.QuestionTypeId);
        }

        private void ConfigureQuestionOptionsTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<QuestionOption>();

            // Table name
            builder.ToTable("question_options");

            // Primary key
            builder.HasKey(qo => qo.Id);

            // Properties
            builder.Property(qo => qo.QuestionId).IsRequired();
            builder.Property(qo => qo.OptionText).IsRequired();
            builder.Property(qo => qo.IsCorrect).IsRequired();
            builder.Property(qo => qo.OptionLetter).IsRequired();

            // Relationships
            builder.HasOne(qo => qo.Question)
                .WithMany(q => q.QuestionOptions)
                .HasForeignKey(qo => qo.QuestionId);
        }

        private void ConfigureQuestionTypesTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<QuestionType>();

            // Table name
            builder.ToTable("question_types");

            // Primary key
            builder.HasKey(qt => qt.Id);

            // Properties
            builder.Property(qt => qt.Name).IsRequired().HasMaxLength(255);

            // Seed data
            builder.HasData(
                new QuestionType { Id = 1, Name = "Multiple Choice" },
                new QuestionType { Id = 2, Name = "True/False" },
                new QuestionType { Id = 3, Name = "Short Answer" },
                new QuestionType { Id = 4, Name = "Text" }
            );
        }

        private void ConfigureQuizAttemptsTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<QuizAttempt>();

            // Table name
            builder.ToTable("quiz_attempts");

            // Primary key
            builder.HasKey(qp => qp.Id);

            // Properties
            builder.Property(qp => qp.StudentId).IsRequired();
            builder.Property(qp => qp.QuizId).IsRequired();
            builder.Property(qp => qp.AttemptStartTimeUTC).IsRequired();
            builder.Property(qp => qp.LastSaved).IsRequired();

            // Map the string value
            builder.Property(qp => qp.State)
                .HasConversion<string>();

            // Relationships
            builder.HasOne(qp => qp.Student)
                .WithMany(u => u.QuizAttempts)
                .HasForeignKey(qp => qp.StudentId);

            builder.HasOne(qp => qp.Quiz)
                .WithMany(q => q.QuizAttempts)
                .HasForeignKey(qp => qp.QuizId);
        }

        private void ConfigureUserResultsTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<UserResult>();

            // Table name
            builder.ToTable("user_results");

            // Primary key
            builder.HasKey(ur => ur.Id);

            // Index
            builder.HasIndex(ur => new { ur.UserId, ur.QuizShortCode })
                   .IsUnique();

            builder.HasIndex(ur => new { ur.UserId, ur.QuizId })
                   .IsUnique();

            // Properties
            builder.Property(ur => ur.UserId).IsRequired();
            builder.Property(ur => ur.QuizId).IsRequired();
            builder.Property(ur => ur.Grade).IsRequired();

            // Relationships
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.Results)
                .HasForeignKey(ur => ur.UserId);

            builder.HasOne(ur => ur.Quiz)
                .WithMany(q => q.UserResults)
                .HasForeignKey(ur => ur.QuizId);
        }

        private void ConfigureStudentQuizSubmissionsTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<StudentQuizSubmission>();

            // Table name
            builder.ToTable("student_quiz_submissions");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.StudentId).IsRequired();
            builder.Property(ur => ur.QuizId).IsRequired();

            // Relationships
            builder.HasOne(ur => ur.Student)
                .WithMany(u => u.QuizSubmissions)
                .HasForeignKey(ur => ur.StudentId);

            builder.HasOne(ur => ur.Quiz)
                .WithMany(q => q.StudentSubmissions)
                .HasForeignKey(ur => ur.QuizId);
        }

        private void ConfigureStudentAnswersTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<StudentAnswer>();

            // Table name
            builder.ToTable("student_answers");

            // Primary key
            builder.HasKey(sa => sa.Id);

            // Properties
            builder.Property(sa => sa.Id).HasColumnName("id");
            builder.Property(sa => sa.UserId).IsRequired();
            builder.Property(sa => sa.QuizId).IsRequired();
            builder.Property(sa => sa.QuestionId).IsRequired();
            builder.Property(sa => sa.AnswerOptionLetter).HasColumnName("answer_option_letter").HasMaxLength(255);
            builder.Property(sa => sa.AnswerText).HasColumnName("answer_text").HasMaxLength(255);

            // Relationships
            builder.HasOne(sa => sa.User)
                .WithMany(u => u.StudentAnswers)
                .HasForeignKey(sa => sa.UserId);

            builder.HasOne(sa => sa.Quiz)
                .WithMany(q => q.StudentAnswers)
                .HasForeignKey(sa => sa.QuizId);

            builder.HasOne(sa => sa.Question)
                .WithMany(q => q.StudentAnswers)
                .HasForeignKey(sa => sa.QuestionId);

            // Check constraint
            builder.HasCheckConstraint("chk_student_answers_answer_not_null", "answer_option IS NOT NULL OR answer_text IS NOT NULL");
        }

        private void ConfigureGroupsTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<Group>();

            // Table name
            builder.ToTable("groups");

            // Primary key
            builder.HasKey(g => g.Id);

            builder.HasIndex(g => g.ShortCode);

            // Properties
            builder.Property(g => g.Name).IsRequired().HasMaxLength(255);
            builder.Property(g => g.CreatedAt).IsRequired();
        }

        private void ConfigureGroupMembersTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<GroupMember>();

            // Table name
            builder.ToTable("group_members");

            // Composite primary key
            builder.HasKey(gm => new { gm.GroupId, gm.MemberId });

            // Properties
            builder.Property(gm => gm.GroupId).HasColumnName("group_id");
            builder.Property(gm => gm.MemberId).HasColumnName("member_id");

            // Relationships
            builder.HasOne(gm => gm.Group)
                .WithMany(g => g.GroupMembers)
                .HasForeignKey(gm => gm.GroupId);

            builder.HasOne(gm => gm.Member)
                .WithMany(m => m.GroupMemberships)
                .HasForeignKey(gm => gm.MemberId);
        }

        private void ConfigureInvitationsTable(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<Invitation>();

            // Table name
            builder.ToTable("invitations");

            // Primary key
            builder.HasKey(i => i.Id);

            // Properties
            builder.Property(i => i.GroupId).IsRequired();
            builder.Property(i => i.InviterId).IsRequired();
            builder.Property(i => i.InviteeEmail).IsRequired().HasMaxLength(255);
            builder.Property(i => i.Status).IsRequired().HasMaxLength(255);
            builder.Property(i => i.CreatedAt).IsRequired();

            // Relationships
            builder.HasOne(i => i.Group)
                .WithMany(g => g.Invitations)
                .HasForeignKey(i => i.GroupId);

            builder.HasOne(i => i.Inviter)
                .WithMany(u => u.SentInvitations)
                .HasForeignKey(i => i.InviterId);

            builder.HasOne(i => i.Invitee)
                .WithMany(u => u.ReceivedInvitations)
                .HasForeignKey(i => i.InviteeEmail)
                .HasPrincipalKey(u => u.Email);
        }
    }
}
