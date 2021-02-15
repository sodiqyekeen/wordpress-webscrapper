using System.Collections.Generic;
using Newtonsoft.Json;

namespace WordPress.Crawler.Shared.Models
{
    public class TagExtractionResponse
    {
        public List<ExtractionResponse> Responses { get; set; }
    }

    public class ExtractionResponse
    {
        public string text { get; set; }
        public object external_id { get; set; }
        public bool error { get; set; }
        public Extraction[] extractions { get; set; }
    }

    public class Extraction
    {
        //[JsonProperty("tag_name")]
        public string tag_name { get; set; }
        public string parsed_value { get; set; }
        public int count { get; set; }
        public string relevance { get; set; }
        public int[] positions_in_text { get; set; }
    }

}
