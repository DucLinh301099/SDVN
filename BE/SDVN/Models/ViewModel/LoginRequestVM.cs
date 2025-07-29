using System.ComponentModel.DataAnnotations;

namespace SDVN.Models.ViewModel
{
    public class LoginRequestVM
    {
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
