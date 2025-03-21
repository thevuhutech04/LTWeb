using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên file")]
        public string FileName { get; set; }

        [Required]
        [Display(Name = "Đường dẫn")]
        public string FilePath { get; set; }

        [Display(Name = "Ảnh chính")]
        public bool IsMain { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
} 