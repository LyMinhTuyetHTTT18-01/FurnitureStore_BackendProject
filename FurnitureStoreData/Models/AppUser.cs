using System.ComponentModel.DataAnnotations;

namespace FurnitureStoreData.Models
{
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MinLength(5, ErrorMessage = "Tên đăng nhập tối thiểu 5 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        // Phân quyền: "Admin" hoặc "Customer"
        public string Role { get; set; } = "Customer";

        public bool IsActive { get; set; } = true;

        // Một User có thể có nhiều đơn hàng (nếu là Customer)
        public virtual ICollection<Order>? Orders { get; set; }

        // Một User có thể viết nhiều bài viết (nếu là Admin)
        public virtual ICollection<Post>? Posts { get; set; }
    }
}