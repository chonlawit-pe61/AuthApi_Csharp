using System.ComponentModel.DataAnnotations;

namespace AuthApi.models
{
    public class UserRegister
    {
        [Required]
        public string username { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;
        [Required]
        public string password { get; set; } = string.Empty;
        [Required]
        public string name { get; set; } = string.Empty;
        [Required]
        public string lname { get; set; } = string.Empty;
        
    }
}
