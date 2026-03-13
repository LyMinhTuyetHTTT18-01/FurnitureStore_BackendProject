using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurnitureStoreData.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên người nhận không được để trống")]
        [MinLength(3, ErrorMessage = "Họ tên phải có ít nhất 3 ký tự")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ (định dạng VN)")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        public string ShippingAddress { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        // Trạng thái đơn: 0 - Chờ xử lý, 1 - Đang giao, 2 - Hoàn thành, 3 - Đã hủy
        public int Status { get; set; } = 0;
        public int? AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }
        // Navigation property
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}