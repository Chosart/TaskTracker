namespace TaskTracker.DTO
{
    public class TaskFilterDto
    {
        public string? Status { get; set; }
        public string? TaskPriority { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int? UserId { get; set; }

    }
}
