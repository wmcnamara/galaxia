using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

public class Lobby : NetworkBehaviour
{
    [SerializeField] private string sceneToLoad = "DevTest";
    [SerializeField] private int maxPlayers = 2;

    private NetworkList<ulong> currentlyConnectedPlayers = new NetworkList<ulong>();

    private const int maxConnectionPayload = 1024;

    public enum ConnectionReturnStatus
    {
        Approved,
        Rejected,
        ServerFull,
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton == null)
        {
            // Can't listen to something that doesn't exist >:(
            throw new Exception($"There is no {nameof(NetworkManager)} for the Lobby system to work with! " +
                $"Please add a {nameof(NetworkManager)} to the scene.");
        }

        if (IsServer)
        {
            foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                currentlyConnectedPlayers.Add(client);
            }

            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (NetworkManager.Singleton == null)
            return;

        if (IsServer)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log("A player with the ID: " + clientId + " just connected to the server!");

        if (IsServer && !currentlyConnectedPlayers.Contains(clientId))
        {
            currentlyConnectedPlayers.Add(clientId);
        }

        EventContainer.Networking.FireOnClientConnected(clientId);
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log("A player with the ID: " + clientId + " just disconnected from the server!");

        if (IsServer && currentlyConnectedPlayers.Contains(clientId))
        {
            currentlyConnectedPlayers.Remove(clientId);
        }

        EventContainer.Networking.FireOnClientDisconnected(clientId);

        //When the host disconnects, we need to handle this
        if (IsClient && clientId == 0)
        {
            HandleHostDisconnect();
        }
    }

    private void HandleHostDisconnect()
    {
        Debug.Log("Host has disconnected. Returning to main menu.");

        NetworkManager.Singleton?.Shutdown();
        SceneManager.LoadScene("Boot");

        return;
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        string playerListStr = "Connected players: " + currentlyConnectedPlayers.Count;

        GUILayout.Label(playerListStr);

        if (IsServer)
        {
            if (GUILayout.Button("Start Game"))
            {
                StartGame();
            }
        }
        else
        {
            GUILayout.Label("Waiting for host to start game...");
        }

        GUILayout.EndVertical();
    }

    private void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }

    public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        byte[] connectionData = request.Payload;

        if (connectionData.Length > maxConnectionPayload)
        {
            // If connectionData too high, deny immediately to avoid wasting time on the server. This is intended as
            // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
            response.Approved = false;
            return;
        }

        ConnectionReturnStatus connectionStatus = DetermineConnectionReturnStatus();

        if (connectionStatus == ConnectionReturnStatus.Approved)
        {
            response.Approved = true;

            response.CreatePlayerObject = true;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;

            return;
        }
        else
        {
            response.Approved = false;
            response.Reason = connectionStatus.ToString();
            return;
        }
    }

    private ConnectionReturnStatus DetermineConnectionReturnStatus()
    {
        //Check if server is full
        if (currentlyConnectedPlayers.Count >= maxPlayers)
        {
            return ConnectionReturnStatus.ServerFull;
        }

        return ConnectionReturnStatus.Approved;
    }
}
