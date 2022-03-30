using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PlinePager.Models
{
    [Table("tblAzans")]
    public class TblAzan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name = "تاریخ")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public string Date { get; set; }

        [Display(Name = "اذن صبح")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public bool EnableA { get; set; }

        public int HourA { get; set; }
        public int MinuteA { get; set; }
        public int SecondA { get; set; }
        public string SoundsBeforeA { get; set; }
        public string SoundsA { get; set; }
        public string SoundsAfterA { get; set; }
        public string AreasA { get; set; }

        [Display(Name = "اذن ظهر")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public bool EnableB { get; set; }

        public int HourB { get; set; }
        public int MinuteB { get; set; }
        public int SecondB { get; set; }
        public string SoundsBeforeB { get; set; }
        public string SoundsB { get; set; }
        public string SoundsAfterB { get; set; }
        public string AreasB { get; set; }

        [Display(Name = "اذن مغرب")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public bool EnableC { get; set; }

        public int HourC { get; set; }
        public int MinuteC { get; set; }
        public int SecondC { get; set; }
        public string SoundsBeforeC { get; set; }
        public string SoundsC { get; set; }
        public string SoundsAfterC { get; set; }
        public string AreasC { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TblAzan>().HasIndex(t => t.Date).IsUnique(true);
        }
    }
}