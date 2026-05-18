namespace MTG_Emulator.Unity.Synchronization.Events;

public class TurnOrderEvent : SyncEvent
{
    public List<string> PlayersNames { get; set; } = new();
    public string CurrentPlayerName { get; set; } = string.Empty;

    public TurnOrderEvent()
    {
        Method = nameof(ISyncEventListener.TurnOrderCreated);
    }
}