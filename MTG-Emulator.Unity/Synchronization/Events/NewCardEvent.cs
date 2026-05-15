using System.Numerics;

namespace MTG_Emulator.Unity.Synchronization.Events
{
    public class NewCardEvent : SyncEvent
    {
        public NewCardEvent()
        {
            Method = nameof(ISyncEventListener.NewCard);
        }

        public Vector2? Position { get; set; }
    }
}
