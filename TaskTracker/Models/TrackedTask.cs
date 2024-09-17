using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models
{
    public class TrackedTask
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string Priority { get; set; }

        [Required]
        public DataType CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        //Foregiven key
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
