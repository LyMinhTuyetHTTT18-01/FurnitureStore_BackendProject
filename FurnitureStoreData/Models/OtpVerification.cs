using System.ComponentModel.DataAnnotations;

namespace FurnitureStoreData.Models
{
    public class OtpVerification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MaxLength(6)]
        public string OtpCode { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime ExpirationTime { get; set; }

        public bool IsUsed { get; set; }
    }
}
