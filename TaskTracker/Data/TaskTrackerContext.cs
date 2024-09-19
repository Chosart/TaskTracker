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
        public DbSet<Models.TrackedTask> TrackedTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackedTask>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId);
        }

        public async Task UpdateCreatedAtValues()
        {
            var tasksToUpdate = await TrackedTasks
                .Where(t => t.CreatedAt != null &&
                            (t.CreatedAt.Ticks < 0 ||
                             t.CreatedAt > new DateTime(9999, 12, 31)))
                .ToListAsync();

            foreach (var task in tasksToUpdate)
            {
                // Припустимо, ви отримали `CreatedAt` як Unix timestamp у вигляді `int` або `long`
                task.CreatedAt = DateTimeOffset.FromUnixTimeSeconds(task.CreatedAt.Ticks).UtcDateTime;
            }

            await SaveChangesAsync();
        }
    }
}
