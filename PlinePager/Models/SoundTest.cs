using System.ComponentModel.DataAnnotations;

namespace PlinePager.Models
{
    public class SoundTest
    {
        [Display(Name = "پیجر")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public int AgentId { get; set; }

        [Display(Name = "فایل صوتی")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        public int SoundId { get; set; }

        [Display(Name = "حجم صدا")]
        [Required(ErrorMessage = "{0} نمی تواند خالی باشد")]
        [Range(-10, 10, ErrorMessage = "بازه انتخاب شده باید بین -10 تا 10 باشد")]
        public int Volume { get; set; }
    }
}