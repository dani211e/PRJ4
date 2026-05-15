

namespace MTG_Emulator.Unity.Synchronization.Events
{
    public class TurnChangedEvent : SyncEvent
    {
        public string  currentPlayerName { get; set; }
        public int turnNumber { get; set; }
        
        public TurnChangedEvent()
        {
            Method = nameof(ISyncEventHandler.OnTurnChanged);
        }
    }
}
