using System;
using System.Linq;
using MTG_Emulator.Cards;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.CardFaceDTO;
using MTG_Emulator.Unity.Db.DTO.RelatedCardDTO;

namespace MTG_Emulator.Cards.Extensions
{
    public static class CardExtensions
    {
        public static CardInfo ToCardInfo(this CardDto c)
        {
            return new CardInfo
            {
                Identifier = Guid.NewGuid(),
                ScryfallId = c.ScryfallId,
                Name = c.Name,
                ImageUri = c.ImageUri,
                AltFace = c.AltFace?.ToCardInfo(),
                RelatedCards = c.RelatedCards?.Select(rc => ToCardInfo((RelatedCardDto)rc)).ToList()
            };
        }

        public static CardFaceInfo ToCardInfo(this CardFaceDto c)
        {
            return new CardFaceInfo
            {
                Name = c.Name,
                ImageUri = c.ImageUri,
            };
        }

        public static RelatedCardInfo ToCardInfo(this RelatedCardDto c)
        {
            return new RelatedCardInfo
            {
                Name = c.Name,
                ImageUri = c.ImageUri,
            };
        }
    }
}