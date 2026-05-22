using System.Numerics;
using MTG_Emulator.Unity.Synchronization.Enums;

namespace MTG_Emulator.Unity.Synchronization.Events
{
    public class MoveCardEvent : ObjectEvent
    {
        public MoveCardEvent(int playerIndex, Guid identifier)
        {
            PlayerIndex = playerIndex;
            Identifier = identifier;
            Method = nameof(ISyncEventListener.MoveCard);
        }

        public Vector2? Position { get; set; }
        public ZoneType Zone { get; set; }
    }
}