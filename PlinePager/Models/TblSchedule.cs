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
        [StringLength(255)]
        public string Areas { get; set; } = null;

        [Display(Name = "صدا های پخش")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [StringLength(255)]
        public string Sounds { get; set; } = null;

        [Display(Name = "حجم صدا")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(-10, 10, ErrorMessage = "بازه انتخاب شده باید بین -10 تا 10 باشد")]
        public int Volume { get; set; }

        [Display(Name = "فعال/غیرفعال")] public bool Enable { get; set; } = true;


        /********************************************************************************/
        [Display(Name = "از تاریخ")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [MaxLength(10, ErrorMessage = "{0} باید 10 کارکتر باشد.")]
        [MinLength(10, ErrorMessage = "{0} باید 10 کارکتر باشد.")]
        public string OfDate { get; set; }

        [Display(Name = "ساعت")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(0, 23, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int OfHour { get; set; }

        [Display(Name = "دقیقه")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(0, 59, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int OfMinute { get; set; }


        [Display(Name = "تکرار دوره زمانی")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public bool IntervalEnable { get; set; }

        [Display(Name = "روز")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(0, 365, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int IntervalDay { get; set; }

        [Display(Name = "ساعت")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(0, 23, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int IntervalHour { get; set; }

        [Display(Name = "دقیقه")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(0, 59, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int IntervalMinute { get; set; }

        [Display(Name = "تا تاریخ")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public bool ToDateEnable { get; set; }

        [Display(Name = "تا تاریخ")]
        //[Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public string ToDate { get; set; }

        [Display(Name = "ساعت")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(0, 23, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int ToHour { get; set; }

        [Display(Name = "دقیقه")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(0, 59, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int ToMinute { get; set; }


        public bool Ended { get; set; } = false;
        public bool Played { get; set; } = false;
        public string NextDate { get; set; }
        public int NextHour { get; set; }
        public int NextMinute { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TblSchedule>().HasIndex(t => t.Name).IsUnique(true);
        }
    }
}