using MTG_Emulator.Unity.Synchronization.Events;

namespace MTG_Emulator.Unity.Synchronization;

public interface ISynchronizationEventHandler
{
    void OnMoveCard(MoveCardEvent e);
    void OnNewCard(NewCardEvent e);
}