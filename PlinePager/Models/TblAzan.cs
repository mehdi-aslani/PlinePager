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

        [Range(0, 23, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        [Display(Name = "ساعت" )]
        public int HourA { get; set; }

        [Range(0, 59, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int MinuteA { get; set; }

        [Range(0, 59, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int SecondA { get; set; }

        [Display(Name = "صوت قبل از اذان" )]
        public string SoundsBeforeA { get; set; }
        
        [Display(Name = "صوت اذان" )]
        public string SoundsA { get; set; }
        
        [Display(Name = "صوت بعد از اذان" )]
        public string SoundsAfterA { get; set; }
        
        [Display(Name = "ناحیه های جهت پخش اذان صبح" )]
        public string AreasA { get; set; }

        [Display(Name = "اذن ظهر")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public bool EnableB { get; set; }

        [Range(0, 23, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        [Display(Name = "ساعت" )]
        public int HourB { get; set; }

        [Range(0, 59, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int MinuteB { get; set; }

        [Range(0, 59, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int SecondB { get; set; }

        [Display(Name = "صوت قبل از اذان" )]
        public string SoundsBeforeB { get; set; }
        
        [Display(Name = "صوت اذان" )]
        public string SoundsB { get; set; }
        
        [Display(Name = "صوت بعد از اذان" )]
        public string SoundsAfterB { get; set; }
        
        [Display(Name = "ناحیه های جهت پخش اذان ظهر" )]
        public string AreasB { get; set; }

        [Display(Name = "اذن مغرب")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public bool EnableC { get; set; }

        [Display(Name = "ساعت" )]
        [Range(0, 23, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int HourC { get; set; }

        [Range(0, 59, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int MinuteC { get; set; }

        [Range(0, 59, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        public int SecondC { get; set; }

        [Display(Name = "صوت قبل از اذان" )]
        public string SoundsBeforeC { get; set; }
        
        [Display(Name = "صوت اذان" )]
        public string SoundsC { get; set; }
        
        [Display(Name = "صوت بعد از اذان" )]
        public string SoundsAfterC { get; set; }
        
        [Display(Name = "ناحیه های جهت پخش اذان مغرب" )]
        public string AreasC { get; set; }
        
        
        [Range(-10, 10, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        [Display(Name = "حجم صدای صبح")]
        public int VolumeA { get; set; } = 0;

        [Range(-10, 10, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        [Display(Name = "حجم صدای ظهر")]
        public int VolumeB { get; set; } = 0;

        [Range(-10, 10, ErrorMessage = "بازه انتخاب شده {0} باید بین {1} تا {2} باشد")]
        [Display(Name = "حجم صدای مغرب")]
        public int VolumeC { get; set; } = 0;

        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TblAzan>().HasIndex(t => t.Date).IsUnique(true);
        }
    }
}