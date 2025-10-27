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
    /// Hanterar anslutningen till SQLite-filen och definierar tabellerna.
    /// </summary>
    public class HangmanDbContext : DbContext // NAMNBYTE
    {
        public DbSet<HighscoreEntry> Highscores { get; set; }
        public DbSet<CustomWordEntry> CustomWords { get; set; } // NYTT: DbSet för anpassade ord

        private readonly string _databasePath;

        public HangmanDbContext()
        {
            // Använder den mapp där det körbara programmet finns (t.ex. bin/Debug/net8.0/)
            string baseDir = AppContext.BaseDirectory;

            // Filen sparas direkt i exekveringsmappen
            _databasePath = Path.Combine(baseDir, "HangmanHighscores.db");

            // Skapa databasfilen och schemat om det inte redan finns
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

            // NYTT: Gör kombinationen av ord och svårighetsgrad unik för CustomWords
            modelBuilder.Entity<CustomWordEntry>()
                .HasIndex(w => new { w.Word, w.Difficulty })
                .IsUnique();
        }
    }
}