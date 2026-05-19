using System.Numerics;
using MTG_Emulator.Unity.Db.DTO.CardDTO;

namespace MTG_Emulator.Unity.Synchronization.Events
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