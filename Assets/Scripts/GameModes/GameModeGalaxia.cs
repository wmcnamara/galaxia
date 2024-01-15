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

        if (IsServer)
        {
            foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                client.PlayerObject.Despawn();

                GameObject playerObject = Instantiate(player);
                playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId, true);
            }           
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
}
