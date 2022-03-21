using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using static PlineFaxServer.Tools.Globals;

namespace PlinePager.Models
{
    [Table("tblAgents")]
    public class TblAgent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [NotNull]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Display(Name = "نوع پیجر")]
        public AgentType Agent { get; set; }


        [NotNull]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [MaxLength(32, ErrorMessage = "حداکثر تعداد کارکتر مجاز برای {0} 32 کاراکتر می باشد")]
        [Display(Name = "شناسه پیجر")]
        public string Username { get; set; }

        [Display(Name = "کلمه عبور")]
        [StringLength(32, MinimumLength = 0, ErrorMessage = "حداکثر تعداد کارکتر مجاز برای {0} 32 کاراکتر می باشد")]
        [NotNull]
        public string Password { get; set; }

        [Display(Name = "عضو ناحیه")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public long AreaId { get; set; }

        [ForeignKey("AreaId")]
        private TblArea Area { get; set; }


        [Display(Name = "توضیحات")]
        [StringLength(128, MinimumLength = 0, ErrorMessage = "حداکثر تعداد کارکتر مجاز برای {0} 128 کاراکتر می باشد")]
        public string Desc { get; set; }

        [Display(Name = "فعال/غیرفعال")]
        public bool Enable { get; set; } = true;


        internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TblAgent>().HasIndex(t => t.Username).IsUnique(true);
        }
    }
}
