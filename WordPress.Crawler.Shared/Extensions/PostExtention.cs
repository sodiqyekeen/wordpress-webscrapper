using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using FastMember;

namespace WordPress.Crawler.Shared.Extensions
{
    public static class PostExtention
    {
        public static Task<string> Clean(this string line)
        {
            string[] headerTags = { "h1", "h2", "h3", "h4", "h5" };
            string[] badWords = { "vanguard</a>", "Vanguard News", "href=\"http://vanguardngr.com\">", "Vanguard Nigeria News", "YOU MAY ALSO LIKE", "ALSO READ", "READ ALSO", "Copyright PUNCH", "All rights reserved", "theeditor@punchng.com", "Ghgossip.com</a", "www.Ghgossip.com</strong", "SEE ALSO:" };
            string response;
            if ((headerTags.Any(t => line.Contains(t)) && line.Contains("By "))
                || (badWords.Any(w => line.Contains(w, StringComparison.OrdinalIgnoreCase))))
                response = string.Empty;
            else
                response = line;

            return Task.FromResult(response);
        }


        public static string ToPlainText(this string postContent)
        {
            List<string> patterns = new List<string>() { @"<script.+?>", @"<video.+?video>", @"<.+?>" };

            patterns.ForEach(p => postContent = Regex.Replace(postContent, p, ""));
            
            return postContent.Trim();
        }
    
        public static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }

            

            return table;
        }
        public static DataTable ToDataTableV2<T>(this IEnumerable<T> data)
        {
            DataTable table = new DataTable();
            using (var reader = ObjectReader.Create(data))
            {
                table.Load(reader);
            }
            return table;
        }
    }
}
