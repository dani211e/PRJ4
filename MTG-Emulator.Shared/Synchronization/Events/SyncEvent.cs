using System;

namespace MTG_Emulator.Shared.Synchronization.Events
{
    public abstract class SyncEvent : EventArgs
    {
        public string Method { get; protected set; } = string.Empty;
    }
}