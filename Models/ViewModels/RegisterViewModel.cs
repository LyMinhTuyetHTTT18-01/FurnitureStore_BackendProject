using System.ComponentModel.DataAnnotations;

namespace FurnitureStoreWeb.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Dòng này không được bỏ trống")]
        [MinLength(5, ErrorMessage = "Tên đăng nhập tối thiểu phải 5 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được bỏ trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu phải 6 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Họ và tên không được bỏ trống")]
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
