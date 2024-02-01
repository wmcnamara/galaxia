using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class GameModeGalaxia : GameModeBase
{
    public int playersNeededToStart = 2;

    private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    private NetworkVariable<bool> gameHasStarted = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        StartGameIfAllPlayersAreConnected();

        EventContainer.GamePlay.OnPlayerDied += OnPlayerDied;
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        EventContainer.GamePlay.OnPlayerDied -= OnPlayerDied;
        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnPlayerDied(ulong playerThatDied)
    {
        Debug.Log("Player with ID: " + playerThatDied + " just died!");

        Invoke(nameof(ResetAfterRoundEnd), 2.0f);
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
        //TODO Refactor AllPlayersConnected to allow the client to view if all players are connected.
        //That way, the the flow here can be used by the client
        if (IsServer)
        {
            if (AllPlayersConnected())
            {
                StartGame();
            }
        }
    }

    private void StartGame()
    {
        if (IsServer && !gameHasStarted.Value)
        {
            ResetPlayers();

            gameHasStarted.Value = true;
        }      
    }

    private void ResetPlayers()
    {
        if (IsServer)
        {
            spawnPoints = FindObjectsOfType<SpawnPoint>().ToList();

            Debug.Assert(NetworkManager.ConnectedClients.Count <= spawnPoints.Count);

            for (int i = 0; i < NetworkManager.ConnectedClientsList.Count; i++)
            {
                int index = i % spawnPoints.Count;

                NetworkClient client = NetworkManager.ConnectedClientsList[index];
                SpawnPoint spawnPoint = spawnPoints[index];

                Player player = client.PlayerObject.GetComponent<Player>();

                player.SetPositionAndRotationClientRpc(spawnPoint.transform.position, spawnPoint.transform.rotation);
                player.ResetLife();

                DestroyAllLasers();
            }
        }
    }

    private void DestroyAllLasers()
    {
        //Host dependent
        DestroyAllLasersClientRpc();
    }

    //I want to die
    [ClientRpc]
    private void DestroyAllLasersClientRpc()
    {
        //This is using the old dumb system to sync the laser
        LaserProjectile[] lasersInScene = FindObjectsOfType<LaserProjectile>();

        foreach (LaserProjectile laser in lasersInScene)
        {
            Destroy(laser.gameObject);
        }
    }

    private void ResetAfterRoundEnd()
    {
        ResetPlayers();

        FindObjectOfType<GameplayUI>().SetLocalOnScreenMessageText("Go!", 0.5f);
    }

    private bool AllPlayersConnected()
    {
        return NetworkManager.ConnectedClients.Count >= playersNeededToStart;
    }
}
