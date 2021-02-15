using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;

namespace WordPress.Crawler.Shared.Models
{
    public class HtmlTags : List<HtmlTag>
    {
        private readonly static string[] tags = { "<!--...-->", "<a>", "<abbr>", "<address>", "<area>", "<article>", "<aside>", "<audio>", "<b>", "<base>", "<bdi>", "<bdo>", "<blockquote>", "<body>", "<br>", "<button>", "<canvas>", "<caption>", "<cite>", "<code>", "<col>", "<colgroup>", "<data>", "<datalist>", "<dd>", "<del>", "<details>", "<dfn>", "<dialog>", "<div>", "<dl>", "<dt>", "<em>", "<embed>", "<fieldset>", "<figcaption>", "<figure>", "<footer>", "<form>", "<h1>", "<h2>", "<h3>", "<h4>", "<h5>", "<h6>", "<head>", "<header>", "<hr>", "<html>", "<i>", "<iframe>", "<img>", "<input>", "<ins>", "<kbd>", "<label>", "<legend>", "<li>", "<link>", "<main>", "<map>", "<mark>", "<meta>", "<meter>", "<nav>", "<noscript>", "<object>", "<ol>", "<optgroup>", "<option>", "<output>", "<p>", "<param>", "<picture>", "<pre>", "<progress>", "<q>", "<rp>", "<rt>", "<ruby>", "<s>", "<samp>", "<script>", "<section>", "<select>", "<small>", "<source>", "<span>", "<strong>", "<style>", "<sub>", "<summary>", "<sup>", "<svg>", "<table>", "<tbody>", "<td>", "<template>", "<textarea>", "<tfoot>", "<th>", "<thead>", "<time>", "<title>", "<tr>", "<track>", "<u>", "<ul>", "<var>", "<video>" };
        public HtmlTags() => AddRange(tags.Select(t => new HtmlTag(t)).ToList());

        //public static List<HtmlTag> Get() => tags.Select(t => new HtmlTag(t)).ToList();

        public bool AnyTag(string value, out HtmlTag tag)
        {
          tag =   this.Where(t => value.Contains(t.CloseTag)).FirstOrDefault();
            if (tag == null)
                return false;
            else
                return true;
        }

        public string RemoveTag(string value, HtmlTag tag)
        {
            StringBuilder tempString = new StringBuilder(value);
            int startIndex = value.IndexOf(tag.OpenTag);
            while (startIndex < value.Length)
            {
                int index = value.IndexOf(tag.OpenTag, startIndex);
                if (index > -1)
                {
                    int length = value.Length - value.IndexOf( ">", startIndex);
                    string temp = value.Substring(index, length);
                    tempString.Replace(temp, "");
                    startIndex = length;
                }
            }
            return tempString.Replace(tag.OpenTag, "").Replace(tag.CloseTag, "").ToString();
        }

        


    }
}
