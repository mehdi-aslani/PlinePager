using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlinePager.Models
{
    [Table("tblSchedules")]
    public class TblSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        [Display(Name = "نام زمان بندی")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public string Name { get; set; }
        
        [Display(Name = "ناحیه ها پخش")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [StringLength(2048)]
        public string Areas { get; set; }
        
        [Display(Name = "صدا های پخش")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [StringLength(2048)]
        public string Sounds { get; set; }
        
        [Display(Name = "زمان بندی")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [StringLength(2048)]
        public string Schedules { get; set; }
        
        [Display(Name = "حجم صدا")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(-10, 10, ErrorMessage = "بازه انتخاب شده باید بین -10 تا 10 باشد")]
        public int Volume { get; set; }
        
        [Display(Name = "فعال/غیرفعال")]
        public bool Enable { get; set; } = true;
        
        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TblSchedule>().HasIndex(t => t.Name).IsUnique(true);
        }
    }
}