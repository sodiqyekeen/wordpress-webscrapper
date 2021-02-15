namespace WordPress.Crawler.Shared.Services
{
    public interface IPublisher
    {
        void RunPostPublisher();
        void RunPostFecther(string url);
    }
}
