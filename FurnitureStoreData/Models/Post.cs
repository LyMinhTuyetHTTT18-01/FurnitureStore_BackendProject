using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurnitureStoreData.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(200, ErrorMessage = "Tiêu đề không được quá 200 ký tự")]
        public string Title { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = true;

        // Khóa ngoại: Bài viết này do Admin nào đăng?
        public int? AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }
    }
}