using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PlinePager.Models.Users
{
    public class TblUser : IdentityUser
    {
        [Display(Name= "نام" )] 
        [MaxLength(128)]
        public string FirstName { get; set; } = "";
        
        [Display(Name="نام خانوادگی")]
        [MaxLength(128)]
        public string LastName { get; set; } = "";

        [Display(Name = "واحد مرتبط")]
        [MaxLength(128)]
        public string Department { get; set; } = "";

        [Display(Name = "وضعیت کاربر")]
        public bool Enable { get; set; } = true;
        
        [Display(Name = "نقش کاربر")]
        public string Role { get; set; }  
        
        [Display(Name = "کاربر باید کلمه عبور خود را تغییر دهد")]
        public bool UserChanePassword { get; set; }
    }
}