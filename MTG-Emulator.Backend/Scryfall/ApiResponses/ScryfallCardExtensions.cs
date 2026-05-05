using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Scryfall
{
    public static class ScryfallCardExtensions
    {
        public static Card ToCard(this ScryfallCard card)
        {
            AltFace? altFace = null;
            if (card.CardFaces != null)
            {
                // Scryfall returns multiface cards' names in a "FRONT // BACK" format
                // so we want to replace the name with the proper one
                card.Name = card.CardFaces[0].Name;
                var apiFace = card.CardFaces[1];
                altFace = new AltFace
                {
                    ImageURI = $"/cards/{card.OracleId}_face1.jpg",
                    OracleText = apiFace.FlavorText ?? string.Empty
                };
            }

            var relatedCards = new List<RelatedCard>();
            if (card.AllParts != null)
                relatedCards.AddRange(card.AllParts.Select(r => new RelatedCard { Name = r.Name, URI = r.Uri }));

            return new Card
            {
                ScryfallId = card.OracleId,
                Name = card.Name,
                ImageUri = $"/cards/{card.OracleId}.jpg",
                OracleText = card.OracleText ?? string.Empty,
                AltFace = altFace,
                RelatedCard = relatedCards
            };
        }
    }
}
