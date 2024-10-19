using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;

namespace TaskTracker.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using TaskTrackerContext dbContext =
                scope.ServiceProvider.GetRequiredService<TaskTrackerContext>();

            dbContext.Database.Migrate();
        }
    }
}
