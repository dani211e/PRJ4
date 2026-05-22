using System.Numerics;
using MTG_Emulator.Shared.Db.DTO.CardDTO;

namespace MTG_Emulator.Shared.Synchronization.Events
{
    public class NewCardEvent : ObjectEvent
    {
        public NewCardEvent(int playerIndex, Guid identifier)
        {
            PlayerIndex = playerIndex;
            Identifier = identifier;
            Method = nameof(ISyncEventListener.NewCard);
        }

        public Vector2? Position { get; set; }
        public CardDto? Card { get; set; }
    }
}