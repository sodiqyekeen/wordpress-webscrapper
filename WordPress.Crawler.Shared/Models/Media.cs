using System.ComponentModel.DataAnnotations;

namespace WordPress.Crawler.Shared.Models
{
    public class Media
    {
        [Key]
        public int MediaId { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public string FilePath { get; set; }
    }
}
