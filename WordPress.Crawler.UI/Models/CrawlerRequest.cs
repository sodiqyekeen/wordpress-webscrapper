using System.ComponentModel.DataAnnotations;

namespace WordPress.Crawler.UI.Models
{
    public class CrawlerRequest
    {
        [Required]
        public string Url { get; set; }
    }
}
