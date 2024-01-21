using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameModeGalaxia : GameModeBase
{
    [SerializeField] private GameObject player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    /*
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnPlayerObjectServerRpc(ulong clientIdToSpawnFor)
    {
        NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientIdToSpawnFor];
        client.PlayerObject?.Despawn();

        GameObject playerObject = Instantiate(player);
        playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientIdToSpawnFor, true);
    }
    */
}
