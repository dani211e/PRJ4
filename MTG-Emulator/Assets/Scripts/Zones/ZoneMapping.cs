using System;
using MTG_Emulator.Unity.Synchronization.Enums;
using UnityEngine;
using UnityEngine.Assertions;

namespace MTG_Emulator.Zones
{
    public class ZoneMapping : MonoBehaviour
    {
        //Note that these should map to the enemy zones
        
        [SerializeField]
        private Transform hand;

        [SerializeField]
        private Transform battlefield;

        [SerializeField]
        private Transform library;

        [SerializeField]
        private Transform graveyard;

        [SerializeField]
        private Transform exile;

        private void Awake()
        {
            Assert.IsNotNull(hand);
            Assert.IsNotNull(battlefield);
            Assert.IsNotNull(library);
            Assert.IsNotNull(graveyard);
            Assert.IsNotNull(exile);
        }

        public Transform GetTransformFor(ZoneType zone)
        {
            return zone switch
            {
                ZoneType.Hand      => hand,
                ZoneType.Bf        => battlefield,
                ZoneType.Library   => library,
                ZoneType.Graveyard => graveyard,
                ZoneType.Exile     => exile,
                _                  => throw new ArgumentOutOfRangeException(nameof(zone), zone, null)
            };
        }
    }
}