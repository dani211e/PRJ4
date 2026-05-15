using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Synchronization.Events;

public class PlayerStatsEvent : SyncEvent
{
    public PlayerStatsEvent()
    {
        Method = nameof(ISyncEventListener.UpdatePlayerStats);
    }

    public int? Health { get; set;}

    public int? LibraryCount { get; set;}
}