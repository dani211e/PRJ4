using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.DB
{
    // ReSharper disable once InconsistentNaming
    public class MTGContext : IdentityDbContext<ApiUser, ApiRole, string>
    {
        public MTGContext() { }

        public MTGContext(DbContextOptions<MTGContext> options) : base(options) { } //Test constructor
        public DbSet<Card> Cards { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<RelatedCard> RelatedCards { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<CardFace> AltFaces { get; set; }
        public DbSet<Game> Games { get; set; } 
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DeckCard>()
                .HasOne(dc => dc.Deck)
                .WithMany(d => d.DeckCards)
                .HasForeignKey(dc => dc.DeckId);

            modelBuilder.Entity<DeckCard>()
                .HasOne(dc => dc.Card)
                .WithMany(c => c.DeckCards)
                .HasForeignKey(dc => dc.CardId);
        }
    }
}
