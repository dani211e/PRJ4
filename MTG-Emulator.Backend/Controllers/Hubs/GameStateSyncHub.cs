using Microsoft.AspNetCore.SignalR;
using MTG_Emulator.Unity.Synchronization;
using MTG_Emulator.Unity.Synchronization.Events;

namespace MTG_Emulator.Backend.Controllers.Hubs
{
    public class GameStateSyncHub : Hub
    {
        public async Task MoveCard(MoveCardEvent e)
        {
            await Clients.Others.SendAsync(nameof(ISynchronizationEventHandler.OnMoveCard), e);
        }

        public async Task NewCard(NewCardEvent e)
        {
            await Clients.Others.SendAsync(nameof(ISynchronizationEventHandler.OnNewCard), e);
        }
    }
}