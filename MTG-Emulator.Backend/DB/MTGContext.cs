using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.DB
{
    public class MTGContext : DbContext
    {
        public DbSet<Card> Cards { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<RelatedCard> RelatedCards { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<AltFace> AltFaces { get; set; }

        public MTGContext(){}

        public MTGContext(DbContextOptions<MTGContext> options) : base(options){} //Test constructor

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (options.IsConfigured) return;

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<MTGContext>()
                .Build();

            string? connectionString = configuration["ConnectionStrings:DefaultConnection"];
            options.UseSqlServer(connectionString);
        }
    }
}
