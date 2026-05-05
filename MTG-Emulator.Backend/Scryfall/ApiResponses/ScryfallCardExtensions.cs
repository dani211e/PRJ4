using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Scryfall
{
    public static class ScryfallCardExtensions
    {
        public static Card ToCard(this ScryfallCard card)
        {
            CardFace? altFace = null;
            bool hasTwoImages = card.ImageUris == null && card.CardFaces != null;
            if (card.CardFaces != null)
            {
                // Scryfall returns multiface cards' names in a "FRONT // BACK" format
                // so we want to replace the name with the proper one
                card.Name = card.CardFaces[0].Name;
                card.OracleText = card.CardFaces[0].OracleText;
                var apiFace = card.CardFaces[1];
                altFace = new CardFace
                {
                    Name = apiFace.Name,
                    ImageUri = hasTwoImages
                        ? $"/cards/{card.OracleId}_face1.jpg"
                        : $"/cards/{card.OracleId}.jpg",
                    OracleText = apiFace.OracleText ?? string.Empty
                };
            }

            var relatedCards = new List<RelatedCard>();
            if (card.AllParts != null)
                relatedCards.AddRange(card.AllParts.Select(r => new RelatedCard { Name = r.Name, ImageUri = r.Uri }));

            return new Card
            {
                ScryfallId = card.OracleId,
                Name = card.Name,
                ImageUri = hasTwoImages
                    ? $"/cards/{card.OracleId}_face0.jpg"
                    : $"/cards/{card.OracleId}.jpg",
                OracleText = card.OracleText ?? string.Empty,
                AltFace = altFace,
                RelatedCards = relatedCards
            };
        }
    }
}
