using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;

namespace TaskTracker.Data
{
    public class TaskTrackerContext : DbContext 
    {
        public TaskTrackerContext(DbContextOptions<TaskTrackerContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<TrackedTask> TrackedTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackedTask>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId);
        }
    }
}
