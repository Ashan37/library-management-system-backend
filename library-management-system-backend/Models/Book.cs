using System.ComponentModel.DataAnnotations;

namespace library_management_system_backend.Models
{
    public class Book
    {
        [Key]
        public int id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string author { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string description { get; set; } = string.Empty;
    }
}
