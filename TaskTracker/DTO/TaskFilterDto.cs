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



    }
}
