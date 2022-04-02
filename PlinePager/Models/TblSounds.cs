using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlinePager.Models
{
    [Table("tblSounds")]
    public class TblSound
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [NotNull]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [MaxLength(64, ErrorMessage = "حداکثر تعداد کارکتر مجاز برای {0} 64 کاراکتر می باشد")]
        [Display(Name = "نام فایل صوتی")]
        public string Name { get; set; }

        [MaxLength(512)]
        [NotNull] public string FileName { get; set; }

        [NotNull]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Display(Name = "فعال/غیرفعال")]
        public bool Enable { get; set; } = true;
        
        public int Length { get; set; }
        
        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TblSound>().HasIndex(t => t.Name).IsUnique(true);
            builder.Entity<TblSound>().HasIndex(t => t.FileName).IsUnique(true);
        }
    }
}