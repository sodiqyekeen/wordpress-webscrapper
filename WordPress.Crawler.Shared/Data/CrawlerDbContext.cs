using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WordPress.Crawler.Shared.Models;
using WordPressPCL.Models;

namespace WordPress.Crawler.Shared.Data
{
    public class CrawlerDbContext : DbContext
    {
        public CrawlerDbContext(DbContextOptions<CrawlerDbContext> options) : base(options)
        {
        }

        public DbSet<WpPost> Posts { get; set; }
        public DbSet<Media> Medias { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Media>()
        //        .HasOne(e => e.Post)
        //        .WithOne(e => e.Media)
        //        .HasForeignKey<WpPost>(e => e.FeaturedMedia);
        //}
    }
}
