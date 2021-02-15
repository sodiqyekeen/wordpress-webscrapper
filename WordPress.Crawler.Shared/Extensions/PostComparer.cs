using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WordPressPCL.Models;

namespace WordPress.Crawler.Shared.Extensions
{
    public class PostComparer : IEqualityComparer<Post>
    {
        public bool Equals([DisallowNull] Post x,  [DisallowNull] Post y)
        {
            return x.Title.Rendered.Equals(y.Title.Rendered, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode([DisallowNull] Post obj)
        {
            return obj.Title.Rendered.GetHashCode();
        }
    }
}
