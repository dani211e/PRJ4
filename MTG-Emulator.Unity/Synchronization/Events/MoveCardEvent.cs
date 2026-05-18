using System.Numerics;

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
    }
}
