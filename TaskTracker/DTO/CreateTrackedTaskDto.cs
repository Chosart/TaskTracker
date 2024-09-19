using System.ComponentModel.DataAnnotations;

namespace TaskTracker.DTO
{
    public class CreateTrackedTaskDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public bool IsCompleted { get; set; }

        [Required]
        public string Priority { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
