using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordPress.Crawler.Shared.Data;
using WordPress.Crawler.Shared.Extensions;
using WordPress.Crawler.Shared.Models;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Utility;

namespace WordPress.Crawler.Shared.Services
{
    public class CrawlerService
    {
        private readonly ILogger<CrawlerService> logger;
        private readonly CrawlerDbContext context;
        //private readonly KeywordExtractionService extractionService;
        private static WordPressSite FreeFund;

        public CrawlerService(ILogger<CrawlerService> _logger, CrawlerDbContext _context)
        {
            logger = _logger;
            context = _context;
            FreeFund = new WordPressSite("https://gist.freefund.xyz/wp-json");
            //extractionService = _extractionService;
        }

        public async Task RemoveDuplicatePosts()
        {


            var posts = FreeFund.ExistingPosts
                .GroupBy(p => p.Title.Rendered)
                .Select(group =>
                new
                {
                    Post = group.Key,
                    Count = group.Count()
                })
                .Where(groupedPost => groupedPost.Count > 1)
                .Select(post => post.Post);

            await FreeFund.Authenticate("admin", "kxB@@0V@04@Z2");

            foreach (var post in posts)
            {
                try
                {
                    var existingPosts = (await FreeFund.GetPostsByTitle(post)).ToArray();

                    for (int i = 1; i < existingPosts.Length; i++)
                    {
                        await FreeFund.DeletePost(existingPosts[i].Id);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, $"An exception occurred process post with title: {post}");
                }
            }
        }

        public async Task FetchPostAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            logger.LogInformation($"About to fetch post from {url}");
            var site = new WordPressClient($"{url}/wp-json");
            logger.LogDebug("Successfully created the WordPress client for the site. About to fetch posts.");
            var queryBuilder = new PostsQueryBuilder
            {
                //Search ="BBNaija",
                After = DateTime.Now.AddDays(-1),
                Before = DateTime.Now,
                PerPage = 50,
                Page = 1
            };
            var posts = (await site.Posts.Query(queryBuilder)).ToList();

            logger.LogDebug("About to login to freefund.");

            await FreeFund.Authenticate("admin", "kxB@@0V@04@Z2");
            foreach (var post in CleanPosts(posts))
            {
                if (post != null)
                {
                    _ = await AddPostAsync(post, await site.Media.GetByID(post.FeaturedMedia.Value));
                }
            }
        }

        //public async Task FetchPostAsync(string url)
        //        {
        //            if (string.IsNullOrEmpty(url))
        //                throw new ArgumentNullException(nameof(url));
        //            logger.LogInformation($"About to fetch post from {url}");
        //            var site = new WordPressClient($"{url}/wp-json");
        //            logger.LogDebug("Successfully created the WordPress client for the site. About to fetch posts.");
        //            var queryBuilder = new PostsQueryBuilder
        //            {
        //                //Search ="BBNaija",
        //                After = DateTime.Now.AddDays(-1),
        //                Before = DateTime.Now,
        //                PerPage = 50,
        //                Page = 1
        //            };
        //            var posts = (await site.Posts.Query(queryBuilder)).ToList();

        //            // logger.LogInformation($"Successfully received {posts.Result.Count()} posts from {url}.");


        //            // var _post = posts.First();
        //            //var keywords = (await extractionService.ExtractKeyword(_post.Content.Rendered)).ToList();



        //            logger.LogDebug("About to login to freefund.");
        //            //WordPressSite freefund = new WordPressSite("https://gist.freefund.xyz/wp-json", "admin", "kxB@@0V@04@Z2");
        //            //WordPressSite freefund = new WordPressSite("https://gist.freefund.xyz/wp-json");
        //            //await freefund.Authenticate("admin", "kxB@@0V@04@Z2");
        //            //logger.LogDebug("Logged in successfully. About to fetch existing posts.");

        //            //var existingPosts = freefund.ExistingPosts;
        //            //logger.LogDebug($"Received {existingPosts.Count()} existing posts. About to remove duplicate posts");
        //            //int duplicate = posts.RemoveAll(p => existingPosts.Any(ep => ep.Title.Rendered.Equals(p.Title.Rendered, StringComparison.OrdinalIgnoreCase)));
        //            //logger.LogInformation($"Removed {duplicate} duplicate posts from the {url} posts.");


        //            //logger.LogDebug($"About to clean posts from {url}.");
        //            //posts = CleanPosts(posts).ToList();

        //            //logger.LogDebug($"Post cleaned successfully. About to add posts from {url} to db.");
        //            //var addPostTask = posts.Select(async post => AddPostAsync(post, await site.Media.GetByID(post.FeaturedMedia.Value)));
        //            //logger.LogDebug($"Post task for {url} started.");
        //            //var newPosts = await Task.WhenAll(addPostTask);

        //            //return newPosts.Where(p => p.Result != null).Select(p => p.Result);

        //            await FreeFund.Authenticate("admin", "kxB@@0V@04@Z2");
        //            foreach (var post in CleanPosts(posts))
        //            {
        //                if (post != null)
        //                {
        //                    var _newPost = await AddPostAsync(post, await site.Media.GetByID(post.FeaturedMedia.Value));
        //                }
        //            }
        //        }

        public async Task PublishPost()
        {
            IEnumerable<WpPost> posts = (await GetPendingPosts()).Take(50);
            await FreeFund.Authenticate("admin", "kxB@@0V@04@Z2");

            foreach (var post in PrepareForPublish(posts))
            {
                try
                {
                    if (post != null)
                    {
                        var _post = await FreeFund.Publish(post);
                        post.Status = 1;
                        await UpdatePostAsync(post);
                    }
                    
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, $"An exception occurred while publishing post with id: {post.PostId}");
                    continue;
                }
            }
        }

        // public async Task PublishPost()
        //{
        //    var posts = (await GetPendingPosts()).Take(10).ToList();
        //    //WordPressSite site = new WordPressSite("https://gist.freefund.xyz/wp-json", "admin", "kxB@@0V@04@Z2");
        //    //var existingPosts = site.ExistingPosts;
        //    //var duplicate = posts.Where(p => existingPosts.Any(ep => ep.Title.Rendered.Equals(p.Title, StringComparison.OrdinalIgnoreCase))).ToList();
        //    //if (duplicate.Any())
        //    //    await DeletePost(duplicate);
        //    //_ = posts.RemoveAll(p => duplicate.Contains(p));
        //    //posts.ForEach(async post =>
        //    //{
        //    //    string filePath = post.Media.FilePath;
        //    //    var _post = await site.AddPost(post);
        //    //    if (_post != null)
        //    //    {
        //    //        DeletePostFile(filePath);
        //    //    }
        //    //});
        //    //try
        //    //{
        //    //    await DeletePost(posts);
        //    //}
        //    //catch (Exception)
        //    //{
        //    //}
        //    await FreeFund.Authenticate("admin", "kxB@@0V@04@Z2");

        //    var AllPosts = FreeFund.ExistingPosts;

        //    foreach (var post in PrepareForPublish(posts))
        //    {
        //        try
        //        {
        //            if (post != null)
        //            {
        //                var _post = await FreeFund.Publish(post);
        //                post.Status = (int)PostStatus.Published;
        //                await UpdatePostAsync(post);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.LogCritical(ex, $"An exception occurred while publishing post with id: {post.PostId}");
        //            continue;
        //        }
        //    }
        //}

        private IEnumerable<WpPost> PrepareForPublish(IEnumerable<WpPost> posts)
        {
            foreach (var post in posts)
            {
                var _post = FreeFund.GetPostByTitle(post.Title).GetAwaiter().GetResult();
                if (_post == null)
                    yield return post;

                post.Status = 2;
                UpdatePost(post);
                yield return null;
            }
        }

        //private void DeletePostFile(string filePath)
        //{
        //    try
        //    {
        //        File.Delete(filePath);
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        private async Task<WpPost> AddPostAsync(Post post, MediaItem media)
        {
            try
            {
                if (await Duplicate(post))
                    return null;

                var _post = new WpPost
                {
                    Title = post.Title.Rendered,
                    Content = post.Content.Rendered,
                    Media = await AddMediaAsync(media),
                    Status = (int)PostStatus.Pending
                };
                await context.Posts.AddAsync(_post);
                _ = await context.SaveChangesAsync();
                return _post;

            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An exception occurred while adding a post.");
                return null;
            }
        }

        private async Task UpdatePostAsync(WpPost post)
        {
            context.Entry(post).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        private void UpdatePost(WpPost post)
        {
            context.Entry(post).State = EntityState.Modified;
            context.SaveChanges();
        }

        //private async Task DeletePost(List<WpPost> posts)
        //{
        //    context.Posts.RemoveRange(posts);
        //    _ = await context.SaveChangesAsync();
        //}


        private async Task<Media> AddMediaAsync(MediaItem media)
        {
            string url = media.SourceUrl;
            string name = url.Substring(url.LastIndexOf('/') + 1).Trim();

            string mediaLocalPath = DownloadFile(url, name);
            var _media = new Media
            {
                Name = name,
                FilePath = mediaLocalPath,
                MimeType = media.MimeType
            };
            await context.Medias.AddAsync(_media);
            _ = await context.SaveChangesAsync();
            return _media;
        }

        private async Task<bool> Duplicate(Post post)
            => await context.Posts.AnyAsync(p => p.Title == post.Title.Rendered);



        public async Task<IEnumerable<WpPost>> GetPosts() =>
            await context.Posts.Include(p => p.Media).ToListAsync();

        public async Task<IEnumerable<WpPost>> GetPendingPosts() =>
            await context.Posts.Where(p => p.Status == 0).Include(p => p.Media).ToListAsync();

        public static IEnumerable<Post> CleanPosts(List<Post> posts)
        {
            foreach (var post in posts)
            {
                if (FreeFund.GetPostByTitle(post.Title.Rendered).GetAwaiter().GetResult() != null)
                    yield return null;

                yield return Clean(post).Result;
            }
        }

        public static string ExtractPostText(WpPost post) =>
            post.Content.ToPlainText();


        private async static Task<Post> Clean(Post post)
        {
            var tempList = post.Content.Rendered.Split("\n");
            var _newPostTask = tempList.Select(l => l.Clean());
            var _newPost = await Task.WhenAll(_newPostTask);

            post.Content.Rendered = string.Join(Environment.NewLine, _newPost);
            return post;
        }

        public static List<WordBagResult> ProduceWordBag(string text)
        {
            var input = new List<Input>
            {
               new Input{Text=text}
            };
            var wordBag = MLProcessor.BagOfWords(input);
            return wordBag;
        }

        private static string GetDownloadPath()
        {
            string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tempFile", DateTime.Now.ToString("ddMMyy"));
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);
            return downloadPath;
        }

        private static string DownloadFile(string url, string fileName)
        {
            string imagePath = Path.Combine(GetDownloadPath(), fileName);
            WebClient client = new WebClient();
            client.DownloadFile(url, imagePath);
            return imagePath;
        }

        //public void RunPostPublisher()
        //{
        //    try
        //    {
        //        PublishPost().GetAwaiter().GetResult();
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}


    }
}
