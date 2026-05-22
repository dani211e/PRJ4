

namespace MTG_Emulator.Shared.Synchronization.Events
{
    public class TurnChangedEvent : SyncEvent
    {
        public string  currentPlayerName { get; set; }
        public int turnNumber { get; set; }
        
        public TurnChangedEvent()
        {
            Method = nameof(ISyncEventListener.TurnChanged);
        }
    }
}
