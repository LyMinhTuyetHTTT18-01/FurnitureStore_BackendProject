using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurnitureStoreData.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [MinLength(5, ErrorMessage = "Tên sản phẩm phải có ít nhất 5 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        [Range(1000, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 1000 VNĐ")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, 10000, ErrorMessage = "Số lượng tồn kho không hợp lệ")]
        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public string? Description { get; set; }

        // Khóa ngoại liên kết với Category
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}