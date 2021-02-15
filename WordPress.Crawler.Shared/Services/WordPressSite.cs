using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordPress.Crawler.Shared.Extensions;
using WordPress.Crawler.Shared.Models;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Utility;

namespace WordPress.Crawler.Shared.Services
{
    public class WordPressSite
    {
        public IEnumerable<Post> ExistingPosts => Site.Posts.GetAll().GetAwaiter().GetResult();
        public IEnumerable<Category> Categories => Site.Categories.GetAll().GetAwaiter().GetResult();
        public WordPressClient Site { get; private set; }

        public WordPressSite(string url)
        {
            Site = new WordPressClient(url);
        }

        public WordPressSite(string url, string username, string password)
        {
            Site = new WordPressClient(url)
            {
                AuthMethod = AuthMethod.JWT
            };
            Site.RequestJWToken(username, password).GetAwaiter().GetResult();
        }

        public async Task Authenticate(string username, string password)
        {
            Site.AuthMethod = AuthMethod.JWT;
            await Site.RequestJWToken(username, password);
            if (!await Site.IsValidJWToken())
                throw new Exception($"Authentication failed for {username}.");
        }

        public async Task<IEnumerable<Post>> GetPosts() =>
            await Site.Posts.GetAll();

        public async Task<IEnumerable<Post>> GetSomePosts()
        {
            var queryBuilder = new PostsQueryBuilder
            {
                After = DateTime.Now.AddDays(-14),
                Before = DateTime.Now.AddDays(-8),
                PerPage = 100,
                Page = 1
            };
            return await Site.Posts.Query(queryBuilder);
        }

        public async Task<IEnumerable<Post>> GetPostsByTitle(string title)
        {
            var queryBuilder = new PostsQueryBuilder
            {
                Search = title
            };
            return await Site.Posts.Query(queryBuilder);
        }

        public async Task<Post> GetPostByTitle(string postTitle)
        {
            var queryBuilder = new PostsQueryBuilder
            {
                Search = postTitle
            };
            var _posts = await Site.Posts.Query(queryBuilder);

            if (_posts.Any())
            {
                return _posts.Where(p => p.Title.Rendered.Equals(postTitle, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }
            return null;
        }

        public async Task DeletePost(int postId)
        {
            await Site.Posts.Delete(postId);
        }

        public async Task<Post> Publish(WpPost post)
        {
            //try
            //{
            int mediaId = await AddMedia(post.Media);
            var _post = ExtractPost(post, mediaId);
            var newPost = Site.Posts.Create(_post).GetAwaiter().GetResult();
            return newPost;
            //}
            //catch (Exception)
            //{
            //    return null;
            //}
        }

        private async Task<int> AddMedia(Media mediaItem)
        {
            MediaItem _newMedia = await Site.Media.Create(mediaItem.FilePath, mediaItem.Name, mediaItem.MimeType);
            return _newMedia.Id;
        }

        private static Post ExtractPost(WpPost sourcePost, int featuredMediaId) =>
            new Post
            {
                Title = new Title(sourcePost.Title.ToUpper()),
                Content = new Content(sourcePost.Content),
                FeaturedMedia = featuredMediaId
                //Tags = sourcePost.Tags
            };

        //private static async Task<string> ExtractContentAsync(WpPost post)
        //{
        //    //return post.Content.Rendered;

        //    var _post = post.Content;
        //    var tempList = _post.Split("\n").ToList();

        //    var _newPostTask = tempList.Select(l => ReplaceLink(l));

        //    var _newPost = await Task.WhenAll(_newPostTask);

        //    return string.Join(Environment.NewLine, _newPost);

        //}

        //private static Task<string> ReplaceLink(string line)
        //{
        //    int index = 0;
        //    while (index < line.Length)
        //    {
        //        int currentTagIndex = line.IndexOf("<a", index);
        //        if (currentTagIndex > 0)
        //        {
        //            string temp = line.Between("<a", ">", index);
        //            line = line.Replace(temp, " href=\"//graizoah.com/afu.php?zoneid=3397384\" target=\"_blank\"");
        //            index = ++currentTagIndex;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    return Task.FromResult(line);
        //}

        //private async Task<IEnumerable<Post>> GetAllPosts() => await Site.Posts.GetAll();

        public bool PostExists(Post post)
        {
            return ExistingPosts.Any(p => p.Title.Rendered.Equals(post.Title.Rendered, StringComparison.OrdinalIgnoreCase));
        }

        //private static Task<string> CleanUpPostLine(string line)
        //{
        //    string[] headerTags = { "h1", "h2", "h3", "h4", "h5" };
        //    string[] badWords = { "vanguard</a>", "<strong>Vanguard Nigeria News</strong></a>", "Vanguard Nigeria News</a>", "READ ALSO" };
        //    string response;
        //    if ((headerTags.Any(t => line.Contains(t)) && line.Contains("By ")) || (badWords.Any(w => line.Contains(w, StringComparison.OrdinalIgnoreCase))))
        //        response = string.Empty;
        //    else
        //        response = line;

        //    return Task.FromResult(response);
        //}



    }
}
