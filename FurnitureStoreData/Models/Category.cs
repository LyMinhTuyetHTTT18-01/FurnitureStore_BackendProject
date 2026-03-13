using System.ComponentModel.DataAnnotations;

namespace FurnitureStoreData.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MinLength(3, ErrorMessage = "Tên danh mục phải có ít nhất 3 ký tự")]
        [MaxLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        public string? Description { get; set; } // Dấu ? cho phép null

        // Navigation property: Một danh mục có nhiều sản phẩm
        public virtual ICollection<Product>? Products { get; set; }
    }
}