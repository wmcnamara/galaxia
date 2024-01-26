using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameModeGalaxia : GameModeBase
{
    public int playersNeededToStart = 2;

    private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        spawnPoints = FindObjectsOfType<SpawnPoint>().ToList();

        if (IsServer)
        {
            StartGameIfAllPlayersAreConnected();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    private void OnClientConnected(ulong clientID)
    {
        if (IsServer)
        {
            StartGameIfAllPlayersAreConnected();
        }
    }

    private void StartGameIfAllPlayersAreConnected()
    {
        if (AllPlayersConnected())
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        Debug.Assert(NetworkManager.ConnectedClients.Count <= spawnPoints.Count);

        for (int i = 0; i < NetworkManager.ConnectedClientsList.Count; i++)
        {
            NetworkClient client = NetworkManager.ConnectedClientsList[i];

            Player player = client.PlayerObject.GetComponent<Player>();

            SpawnPoint spawnPoint = spawnPoints[i];

            player.SetPositionAndRotationClientRpc(spawnPoint.transform.position, spawnPoint.transform.rotation);
        } 
    }

    private bool AllPlayersConnected()
    {
        return NetworkManager.ConnectedClients.Count >= playersNeededToStart;
    }
}
