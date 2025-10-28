using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hangman.Core.Models;
using System.IO;

namespace Hangman.Core.Providers.Db
{
    /// <summary>
    /// EF Core databas-kontext för Highscores och Custom Words.
    /// </summary>
    public class HangmanDbContext : DbContext
    {
        public DbSet<HighscoreEntry> Highscores { get; set; }
        public DbSet<CustomWordEntry> CustomWords { get; set; }

        private readonly string _databasePath;

        public HangmanDbContext()
        {
            string baseDir = AppContext.BaseDirectory;
            _databasePath = Path.Combine(baseDir, "Hangman.db");

            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Gör kombinationen av namn och svårighetsgrad unik för Highscores
            modelBuilder.Entity<HighscoreEntry>()
                .HasIndex(h => new { h.PlayerName, h.Difficulty })
                .IsUnique();

            // Gör kombinationen av ord, svårighetsgrad OCH språk unik
            modelBuilder.Entity<CustomWordEntry>()
                .HasIndex(w => new { w.Word, w.Difficulty, w.Language })
                .IsUnique();
        }
    }
}