using System.ComponentModel.DataAnnotations;

namespace PlinePager.Models
{
    public class LoginPage
    {
        [Display(Name = "نام کاربری")]
        [Required(ErrorMessage = "نام کاربری را وارد کنید")]
        public string Username { get; set; }

        [Display(Name = "کلمه عبور")]
        [Required(ErrorMessage = "کلمه عبور را وارد کنید"), DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "مرا به خاطر بسپار")] public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}