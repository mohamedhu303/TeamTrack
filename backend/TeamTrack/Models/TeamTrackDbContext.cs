using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace TeamTrack.Models
{
    public class TeamTrackDbContext : IdentityDbContext<ApplicationUser>
    {
        public TeamTrackDbContext(DbContextOptions<TeamTrackDbContext> options) : base(options)
        {

        }

        public DbSet<ApplicationUser> user { get; set; }
        public DbSet<Project> project { get; set; }
        public DbSet<UserTask>userTask { get; set; }
        public DbSet<ApplicationUser> applicationUsers { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            // Create CreatedDate is not able to edit
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.createdDate)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            // ApplicationUser
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.createdDate)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

            // Project -> Tasks
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Tasks)
                .WithOne(t => t.project)
                .HasForeignKey(t => t.projectId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Tasks (AssignedUser)
            modelBuilder.Entity<UserTask>()
                .HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    }
