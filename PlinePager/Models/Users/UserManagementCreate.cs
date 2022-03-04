using System.ComponentModel.DataAnnotations;

namespace PlinePager.Models.Users
{
    public class UserManagementCreate
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "{0} را وارد کنید")]
        [Display(Name = "نام")]
        [MaxLength(128)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "{0} را وارد کنید")]
        [Display(Name = "نام خانوادگی")]
        [MaxLength(128)]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "{0} را وارد کنید")]
        [Display(Name = "نام کاربری")]
        [MaxLength(256)]
        public string Username { get; set; } = "";

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} را وارد کنید")]
        [Display(Name = "کلمه عبور")]
        [MaxLength(256)]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} را وارد کنید")]
        [Display(Name = "تکرار کلمه عبور")]
        [MaxLength(256)]
        public string RepPassword { get; set; } = "";
        
        [Display(Name = "واحد مرتبط")]
        [MaxLength(128)]
        public string Department { get; set; } = "";
        
        [Display(Name = "کاربر باید کلمه عبور خود را تغییر دهد")]
        public bool UserChanePassword { get; set; }
        
    }
}