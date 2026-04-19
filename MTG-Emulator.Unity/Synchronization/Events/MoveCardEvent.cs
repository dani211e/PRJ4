using System.Numerics;

namespace MTG_Emulator.Unity.Synchronization.Events
{
    public class MoveCardEvent(string client, string card, Vector2 position) : SyncCardEvent(client, card)
    {
        public Vector2 Position { get; } = position;
    }
}
