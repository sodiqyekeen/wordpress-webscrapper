using System;
using System.Collections.Generic;
using System.Text;

namespace WordPress.Crawler.Shared.Models
{
    public class HtmlTag
    {
        public string Tag { get; set; }
        public string OpenTag { get; private set; }
        public string CloseTag { get; private set; }
        //public int? OpenTagIndex { get; set; }
        //public int? CloseTagIndex { get; set; }


        public HtmlTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));
            Tag = tag;


            switch (Tag)
            {
                case "<!--...-->":
                    OpenTag = "<!--";
                    CloseTag = "-->";
                    break;
                case "<img>":
                    OpenTag = "<img";
                    CloseTag = "/>";
                    break;

                default:
                    OpenTag = Tag[0..^1];
                    CloseTag = $"</{Tag[1..^1]}>";
                    break;
            }


        }
    }
}
