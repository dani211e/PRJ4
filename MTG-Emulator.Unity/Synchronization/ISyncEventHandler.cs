using MTG_Emulator.Unity.Synchronization.Events;

namespace MTG_Emulator.Unity.Synchronization;

public interface ISyncEventHandler
{
    void OnMoveCard(MoveCardEvent e);
    void OnNewCard(NewCardEvent e);
    void OnUpdatePlayerStats(PlayerStatsEvent e);
    void OnTurnChanged(TurnChangedEvent e);
    void OnTurnOrderCreated(TurnOrderEvent e);
    void OnTapCard(TapCardEvent e);
}
