namespace MTG_Emulator.Unity.Synchronization.Events;

public class PlayerStatsEvent : ObjectEvent
{
    public PlayerStatsEvent(int playerIndex)
    {
        PlayerIndex = playerIndex;
        Method = nameof(ISyncEventListener.UpdatePlayerStats);
    }

    public int? Health { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public int? LibraryCount { get; set;}
}
