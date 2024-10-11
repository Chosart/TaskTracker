using TaskTracker.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TaskTracker.DTO
{
    public class TaskFilterDto
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int? Limit { get; set; }
        public List<string> Statuses { get; set; } = new();
        public int? UserId { get; set; }

        public List<TrackedTask> FilterTrackedTasks(List<TrackedTask> tasks, int? priority, string? status)
        {
            // Фільтрація за статусами
            if (Statuses != null && Statuses.Any())
            {
                tasks = tasks.Where(task => Statuses.Contains(task.Status)).ToList();
            }

            // Фільтрація за статусом
            if (!string.IsNullOrEmpty(status))
            {
                tasks = tasks.Where(task => task.Status == status).ToList();
            }

            // Фільтрація за пріоритетом
            var priorityString = priority switch
            {
                1 => "High",
                2 => "Medium",
                3 => "Low",
                _ => null
            };

            if (!string.IsNullOrEmpty(priorityString))
            {
                tasks = tasks.Where(task => task.Priority == priorityString).ToList();
            }

            // Застосування ліміту
            if (Limit.HasValue && Limit.Value > 0)
            {
                tasks = tasks.Take(Limit.Value).ToList();
            }

            if (CreatedBefore.HasValue)
            {
                var createdBeforeUnix = ((DateTimeOffset)CreatedBefore.Value).ToUnixTimeSeconds();
                tasks = tasks.Where(task => task.CreatedAt <= createdBeforeUnix).ToList();
            }

            return tasks ?? new List<TrackedTask>();
        }
    }
}
