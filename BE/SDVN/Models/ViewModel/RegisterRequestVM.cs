using System.ComponentModel.DataAnnotations;

namespace SDVN.Models.ViewModel
{
    public class RegisterRequestVM
    {
        [Required]  
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
