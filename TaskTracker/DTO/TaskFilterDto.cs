using TaskTracker.Models;

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

            return tasks;
        }
    }
}
