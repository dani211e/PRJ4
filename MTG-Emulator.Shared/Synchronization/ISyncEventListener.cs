using MTG_Emulator.Unity.Synchronization.Events;
using System.Threading.Tasks;

namespace MTG_Emulator.Unity.Synchronization;

public interface ISyncEventListener
{
    Task MoveCard(MoveCardEvent e);
    Task NewCard(NewCardEvent e);
    Task UpdatePlayerStats(PlayerStatsEvent e);
    Task TurnChanged(TurnChangedEvent e);
    Task TurnOrderCreated(TurnOrderEvent e);
}
