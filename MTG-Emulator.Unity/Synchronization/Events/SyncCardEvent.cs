namespace MTG_Emulator.Unity.Synchronization.Events
{
    public class SyncCardEvent(string client, string card)
    {
        public string Card { get; } = card;
        public string Client  { get; } = client;

    }
}
