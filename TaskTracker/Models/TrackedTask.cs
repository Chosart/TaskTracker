using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models
{
    public class TrackedTask
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public bool IsCompleted { get; set; }

        [Required]
        public string Priority { get; set; }

        [Required]
        public int CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }
    }
}
