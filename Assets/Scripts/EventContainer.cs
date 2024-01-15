using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class EventContainer
{
    public static class GamePlay
    {
        public static event UnityAction<ulong> OnPlayerDied;

        public static void FireOnPlayerDied(ulong playerID)
        {
            OnPlayerDied?.Invoke(playerID);
        }
    }

    public static class Networking
    {
        public static event UnityAction<ulong> OnClientConnected;
        public static event UnityAction<ulong> OnClientDisconnected;

        public static void FireOnClientConnected(ulong playerID)
        {
            OnClientConnected?.Invoke(playerID);
        }

        public static void FireOnClientDisconnected(ulong playerID)
        {
            OnClientDisconnected?.Invoke(playerID);
        }
    }
}
