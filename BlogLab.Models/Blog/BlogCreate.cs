using System.ComponentModel.DataAnnotations;

namespace BlogLab.Models.Blog
{
    public class BlogCreate
    {
        public int BlogId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MinLength(10, ErrorMessage = "Must be at least 10 characters")]
        [MaxLength(50, ErrorMessage = "Must be at most 50 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MinLength(300, ErrorMessage = "Must be at least 300 characters")]
        [MaxLength(3000, ErrorMessage = "Must be at most 3000 characters")]
        public string Content { get; set; }
        public int? PhotoId { get; set; }
    }
}