using Microsoft.AspNetCore.SignalR;
using MTG_Emulator.Unity.Synchronization;
using MTG_Emulator.Unity.Synchronization.Events;

namespace MTG_Emulator.Backend.Controllers.Hubs
{
    public class GameStateSyncHub : Hub, ISyncEventListener
    {
        public async Task MoveCard(MoveCardEvent e)
        {
            await Clients.Others.SendAsync(nameof(ISyncEventHandler.OnMoveCard), e);
        }

        public async Task NewCard(NewCardEvent e)
        {
            await Clients.Others.SendAsync(nameof(ISyncEventHandler.OnNewCard), e);
        }

        public async Task UpdatePlayerStats(PlayerStatsEvent e)
        {
            await Clients.Others.SendAsync(nameof(ISyncEventHandler.OnUpdatePlayerStats), e);
        }

        public async Task TurnChanged(TurnChangedEvent e)
        {
            await Clients.All.SendAsync(nameof(ISyncEventHandler.OnTurnChanged), e);
        }

        public async Task TurnOrderCreated(TurnOrderEvent e)
        {
            await Clients.All.SendAsync(nameof(ISyncEventHandler.OnTurnOrderCreated), e);
        }

        public async Task PlayerLeave(PlayerLeaveEvent e)
        {
            await Clients.All.SendAsync(nameof(ISyncEventHandler.OnPlayerLeave), e);
        }
    }
}
