using MTG_Emulator.Unity.Synchronization.Events;

namespace MTG_Emulator.Unity.Synchronization.Events;

public abstract class ObjectEvent : SyncEvent
{
    public Guid Identifier { get; protected set; } = Guid.Empty;
    public int PlayerIndex { get; protected set; } = -1;
}