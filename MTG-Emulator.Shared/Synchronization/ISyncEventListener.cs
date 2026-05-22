using System.Threading.Tasks;
using MTG_Emulator.Shared.Synchronization.Events;

namespace MTG_Emulator.Shared.Synchronization;

public interface ISyncEventListener
{
    Task MoveCard(MoveCardEvent e);
    Task NewCard(NewCardEvent e);
    Task UpdatePlayerStats(PlayerStatsEvent e);
    Task TurnChanged(TurnChangedEvent e);
    Task TurnOrderCreated(TurnOrderEvent e);
}
