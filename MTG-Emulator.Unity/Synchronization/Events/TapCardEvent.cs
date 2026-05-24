using System;

namespace MTG_Emulator.Unity.Synchronization.Events
{
    public class TapCardEvent : ObjectEvent
    {
        public TapCardEvent(int playerIndex, Guid identifier, bool isTapped)
        {
            PlayerIndex = playerIndex;
            Identifier = identifier;
            IsTapped = isTapped;
            Method = nameof(ISyncEventListener.TapCard);
        }

        public bool IsTapped { get; set; }
    }
}