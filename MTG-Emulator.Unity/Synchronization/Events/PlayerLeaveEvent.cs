namespace MTG_Emulator.Unity.Synchronization.Events;

public class PlayerLeaveEvent : ObjectEvent
{
    public PlayerLeaveEvent(int playerIndex)
    {
        Identifier = Guid.Empty;
        PlayerIndex = playerIndex;
        Method = nameof(ISyncEventListener.PlayerLeave);
    }

    public List<string> PlayersNames { get; set; } = new();
}