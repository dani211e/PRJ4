using System.Numerics;

namespace MTG_Emulator.Unity.Synchronization.Events
{
    public class MoveCardEvent : SyncEvent
    {
        public MoveCardEvent()
        {
            Method = nameof(ISyncEventListener.MoveCard);
        }


        public Vector2? Position { get; set; }
    }
}
