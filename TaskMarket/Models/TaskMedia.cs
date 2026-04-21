using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models
{
    public class TaskMedia
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public virtual TaskItem TaskItem { get; set; }

        [Required]
        public string MediaUrl { get; set; }

        // e.g., "Image" or "Video"
        [Required]
        public string MediaType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
