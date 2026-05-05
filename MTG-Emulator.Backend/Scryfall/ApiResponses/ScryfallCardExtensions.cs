using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.Scryfall.ApiResponses
{
    public static class ScryfallCardExtensions
    {
        public static Card ToCard(this ScryfallCard card, Dictionary<Guid, Guid> idToOracleId)
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
            {
                var gameComponents = new HashSet<string> { "token", "emblem" };
                relatedCards.AddRange(
                    card.AllParts
                        .Where(r => gameComponents.Contains(r.Component))
                        .Select(r =>
                        {
                            var scryfallId = r.Id;
                            idToOracleId.TryGetValue(r.Id, out var oracleId);
                            return new RelatedCard
                            {
                                Name = r.Name,
                                ImageUri = oracleId != default ? $"/cards/{oracleId}.jpg" : string.Empty
                            };
                        })
                );
            }

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
