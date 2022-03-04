using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlinePager.Models
{
    [Table("tblAreas")]
    public class TblArea
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [NotNull]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [MaxLength(32, ErrorMessage = "حداکثر تعداد کارکتر مجاز برای {0} 32 کاراکتر می باشد")]
        [Display(Name = "ناحیه")]
        public string Name { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(128, MinimumLength = 0, ErrorMessage = "حداکثر تعداد کارکتر مجاز برای {0} 128 کاراکتر می باشد")]
        public string Desc { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TblArea>().HasIndex(t => t.Name).IsUnique(true);
        }
    }
}
