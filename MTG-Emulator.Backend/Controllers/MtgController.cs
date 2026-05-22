using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Shared.Db.DTO.CardDTO;
using MTG_Emulator.Shared.Db.DTO.CardFaceDTO;
using MTG_Emulator.Shared.Db.DTO.RelatedCardDTO;

namespace MTG_Emulator.Backend.Controllers
{
    public class MtgController : ControllerBase
    {
        public bool IsOwnerOrAdmin(string resourceApiUserId)
        {
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return resourceApiUserId == callerId || User.IsInRole(Roles.Admin);
        }

        protected static CardDto ToCardDto(Card card) => new()
        {
            CardId = card.CardId,
            ScryfallId = card.ScryfallId,
            Name = card.Name,
            OracleText = card.OracleText,
            ImageUri = card.ImageUri,
            AltFace = card.AltFace == null ? null : new CardFaceDto
            {
                Name = card.AltFace.Name,
                OracleText = card.AltFace.OracleText,
                ImageUri = card.AltFace.ImageUri,
            },
            RelatedCards = card.RelatedCards.Select(rc => new RelatedCardDto
            {
                RelatedCardId = rc.RelatedCardId,
                Name = rc.Name,
                ImageUri = rc.ImageUri,
            }).ToList(),
        };
    }
}