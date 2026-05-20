using UnityEngine;

namespace MTG_Emulator.Extensions
{
    public static class GameObjectExtensions
    {
        public static void RemoveComponent<T>(this GameObject obj) where T : MonoBehaviour
        {
            var comp = obj.GetComponent<T>();
            
            if(!comp)
                Debug.LogWarning($"Attempted to remove component {nameof(T)} from {obj.name}, but it was not found!");

            Object.Destroy(comp);
        }
    }
}