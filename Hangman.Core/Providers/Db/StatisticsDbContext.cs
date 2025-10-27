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
    /// EF Core databas-kontext för Highscores.
    /// Hanterar anslutningen till SQLite-filen och definierar tabellerna.
    /// </summary>
    public class StatisticsDbContext : DbContext
    {
        // DbSet representerar tabellen i databasen
        public DbSet<HighscoreEntry> Highscores { get; set; }

        private readonly string _databasePath;

        public StatisticsDbContext()
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
            // Konfigurera EF Core att använda SQLite och peka på filen
            optionsBuilder.UseSqlite($"Data Source={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Gör kombinationen av namn och svårighetsgrad unik för att undvika dubbletter
            modelBuilder.Entity<HighscoreEntry>()
                .HasIndex(h => new { h.PlayerName, h.Difficulty })
                .IsUnique();
        }
    }
}