using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace MTG_Emulator.Threading
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
        
        private void Awake() => DontDestroyOnLoad(gameObject);

        private void Update()
        {
            if (!actions.TryDequeue(out var action))
                return;

            action();
            Debug.Log("Called action");
        }

        public static void Enqueue(Action action) => actions.Enqueue(action);
    }
}