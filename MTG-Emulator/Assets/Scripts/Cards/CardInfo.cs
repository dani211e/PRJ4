using System;
using System.Collections.Generic;
using MTG_Emulator.Unity.Db.DTO.CardFaceDTO;
using MTG_Emulator.Unity.Db.DTO.RelatedCardDTO;

namespace MTG_Emulator.Cards
{
    public class CardInfo
    {
        public Guid Identifier { get; set; }
        public Guid ScryfallId { get; set; }
        public string Name { get; set; } = string.Empty;
        // public string OracleText { get; set; } = string.Empty;
        public string ImageUri { get; set; } = string.Empty;

        public CardFaceInfo AltFace { get; set; }
        public List<RelatedCardInfo> RelatedCards { get; set; }
    }
}