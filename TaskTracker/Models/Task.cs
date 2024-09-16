using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string Priority { get; set; }
        public DataType CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        //Foregiven key
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
