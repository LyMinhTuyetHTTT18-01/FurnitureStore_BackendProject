using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurnitureStoreData.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng mua phải từ 1 đến 100")]
        public int Quantity { get; set; }

        // Lưu lại giá tại thời điểm mua, đề phòng sau này Product tăng/giảm giá
        [Required]
        public decimal UnitPrice { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}