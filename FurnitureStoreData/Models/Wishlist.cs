using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurnitureStoreData.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser? AppUser { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}
