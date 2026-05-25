

namespace MTG_Emulator.Unity.Synchronization.Events
{
    public class TurnChangedEvent : SyncEvent
    {
        public string  CurrentPlayerName { get; set; } = string.Empty;
        public int TurnNumber { get; set; }
        
        public TurnChangedEvent()
        {
            Method = nameof(ISyncEventListener.TurnChanged);
        }
    }
}
