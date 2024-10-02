namespace TaskTracker.DTO
{
    public class TaskFilterDto
    {
        public string? Status { get; set; }
        public string? Proirity { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

    }
}
