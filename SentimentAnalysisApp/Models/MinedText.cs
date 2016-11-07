using System;
using System.Data.Entity;

namespace SentimentAnalysisApp.Models
{
    public enum Source
    {
        Twitter, Other
    }

    public class MinedText
    {
        public int ID { get; set; }
        public string TheText { get; set; }
        public string TheCreator { get; set; }
        public Source TheSource { get; set; }

    }

/*
    public class MinedDataContext : DbContext
    {
        public DbSet<MinedText> MinedTexts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MinedText>().ToTable("MinedText");
        }
    }
 */
}