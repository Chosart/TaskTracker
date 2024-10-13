namespace TaskTracker.DTO
{
    public class TrackedTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
    }
}
