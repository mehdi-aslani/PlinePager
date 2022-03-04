using System.ComponentModel.DataAnnotations;

namespace PlinePager.Models.Users
{
    public class ChangePassword
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} را وارد کنید")]
        [Display(Name = "کلمه عبور فعلی")]
        [MaxLength(256)]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} را وارد کنید")]
        [Display(Name = "کلمه عبور جدید")]
        [MaxLength(256)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} را وارد کنید")]
        [Display(Name = "تکرار کلمه عبور جدید")]
        [MaxLength(256)]
        public string RepPassword { get; set; }
    }
}