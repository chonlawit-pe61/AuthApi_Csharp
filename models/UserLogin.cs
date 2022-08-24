using System.ComponentModel.DataAnnotations;

namespace AuthApi.models
{
    public class UserLogin
    {
        [Required]
        public string username { get; set; } = string.Empty;
        [Required]
        public string password { get; set; } = string.Empty;
    }
}