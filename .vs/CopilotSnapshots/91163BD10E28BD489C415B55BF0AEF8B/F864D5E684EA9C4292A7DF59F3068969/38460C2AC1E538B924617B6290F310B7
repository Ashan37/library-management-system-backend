using System.ComponentModel.DataAnnotations;

namespace library_management_system_backend.Models
{
    public class User
    {
        [Key]
        public int id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string password { get; set; } = string.Empty;
	}
}
