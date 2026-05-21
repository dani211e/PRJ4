using MTG_Emulator.Shared.Synchronization.Events;

namespace MTG_Emulator.Shared.Synchronization.Events;

public abstract class ObjectEvent : SyncEvent
{
    public Guid Identifier { get; protected set; } = Guid.Empty;
    public int PlayerIndex { get; protected set; } = -1;
}