namespace MTG_Emulator.Unity.Synchronization.Events
{
    public class SyncEvent : EventArgs
    {
        public string Method { get; protected set; } = string.Empty;
    }
}