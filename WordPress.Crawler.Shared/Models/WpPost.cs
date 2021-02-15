using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordPress.Crawler.Shared.Models
{
    public class WpPost
    {
        [Key]
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        [ForeignKey("MediaId")]
        public Media Media { get; set; }
        public int Status { get; set; }
    }
}
