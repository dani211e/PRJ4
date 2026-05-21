using MTG_Emulator.Shared.Synchronization.Events;

namespace MTG_Emulator.Shared.Synchronization;

public interface ISyncEventHandler
{
    void OnMoveCard(MoveCardEvent e);
    void OnNewCard(NewCardEvent e);
    void OnUpdatePlayerStats(PlayerStatsEvent e);
    void OnTurnChanged(TurnChangedEvent e);
    void OnTurnOrderCreated(TurnOrderEvent e);
}
