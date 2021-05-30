using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Questionaire.Models;

namespace Questionaire.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Question> Questions { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<Answer> Answers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=TestDatabase.db", options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map table names
            modelBuilder.Entity<Question>().ToTable("Questions", "ApplicationDB");
            modelBuilder.Entity<Choice>().ToTable("Choices", "ApplicationDB");
            modelBuilder.Entity<Answer>().ToTable("Answers", "ApplicationDB");
            base.OnModelCreating(modelBuilder);
        }
    }
}
