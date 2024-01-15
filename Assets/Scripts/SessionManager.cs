using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.SceneManagement;

public class SessionManager : NetworkBehaviour
{
    public static SessionManager Instance { get; private set; }

    private List<Player> playerListCache = new List<Player>();
    private bool playerListCacheIsClean = false;

    private Dictionary<ulong, NetworkObjectReference> currentlyConnectedClients = new Dictionary<ulong, NetworkObjectReference>();

    //Returns a list of all Player scripts attached to connected clients.
    //This does NOT include clients that are connected without a Player script.
    public List<Player> CurrentlyConnectedPlayers
    {
        get 
        { 
            if (!playerListCacheIsClean)
            {
                UpdateConnectedPlayerListCache();
            }
            
            return playerListCache;
        }
    }

    public List<Player> CurrentlyConnectedPlayersExceptLocal
    {
        get
        {
            if (!playerListCacheIsClean)
            {
                UpdateConnectedPlayerListCache();
            }

            return playerListCache.Where(player => player.OwnerClientId != NetworkManager.LocalClientId).ToList();
        }
    }

    private void UpdateConnectedPlayerListCache()
    {
        //Fill the playerlist
        playerListCache.Clear();

        foreach (NetworkObjectReference clientNetworkObject in currentlyConnectedClients.Values)
        {
            if (clientNetworkObject.TryGet(out NetworkObject playerNetObject))
            {
                if (playerNetObject.TryGetComponent(out Player player))
                {
                    playerListCache.Add(player);
                }
            }
        }

        playerListCacheIsClean = true;
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    //Keeping to allow easy testing
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("----------");
            foreach (ulong id in currentlyConnectedClients.Keys)
            {
                if (currentlyConnectedClients[id].TryGet(out NetworkObject playerObject))
                {
                    Debug.Log("Client ID: " + id + " Position of client: " + playerObject.transform.position + " name: " + playerObject.transform.name);
                }
            }
            Debug.Log("----------");
            foreach (Player player in CurrentlyConnectedPlayers)
            {
                Debug.Log(" Position of player: " + player.transform.position + " name: " + player.transform.name);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        EventContainer.GamePlay.OnPlayerDied += OnPlayerDied;

        RequestConnectedClientDataServerRpc(OwnerClientId);

        playerListCacheIsClean = false;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        EventContainer.GamePlay.OnPlayerDied -= OnPlayerDied;
    }

    private void OnPlayerDied(ulong player)
    {
        playerListCacheIsClean = false;
    }

    private void OnClientConnected(ulong clientID)
    {
        Debug.Log("Client with the ID: " + clientID + " has connected to the server");

        RequestConnectedClientDataServerRpc(clientID);
    }

    private void OnClientDisconnected(ulong clientID)
    {
        Debug.Log("Client with the ID: " + clientID + " has disconnected from the server");

        //Handle host disconnect
        //TODO broken with a server (I think?)
        if (clientID == 0)
        {
            NetworkManager.Shutdown();
            SceneManager.LoadScene("MainMenu");
            return;
        }

        //When a client disconnects, remove them from the currently connected players.
        if (currentlyConnectedClients.ContainsKey(clientID))
        {
            currentlyConnectedClients.Remove(clientID);
        }

        playerListCacheIsClean = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestConnectedClientDataServerRpc(ulong clientID)
    {
        NetworkObjectReference clientObject = NetworkManager.ConnectedClients[clientID].PlayerObject;

        ReturnConnectedClientDataClientRpc(clientID, clientObject);
    }

    [ClientRpc]
    private void ReturnConnectedClientDataClientRpc(
        ulong clientData, 
        NetworkObjectReference networkClient, 
        ClientRpcParams clientRpcParams = default)
    {
        currentlyConnectedClients[clientData] = networkClient;
        playerListCacheIsClean = false; //Invalidate the cache only when we recieve the connection data from the rpc
    }

    //Returns a reference to a player with a client ID. Can be called on both client and server.
    public Player GetPlayerFromNetworkClientID(ulong playerOwnerClientID)
    {
        if (currentlyConnectedClients.ContainsKey(playerOwnerClientID))
        {
            if (currentlyConnectedClients[playerOwnerClientID].TryGet(out NetworkObject playerNetObject))
            {
                if (playerNetObject.TryGetComponent(out Player player))
                {
                    return player;
                }
            }
            else
            {
                Debug.LogWarning($"GameManager.{nameof(GetPlayerFromNetworkClientID)} called with an ID that doesnt exist on the server");
            }
        }

        return null;
    }

    //Returns a reference to the local player if they have a Player script on their playerobject. Returns null if not.
    public Player GetLocalPlayer()
    {
        return NetworkManager.LocalClient.PlayerObject.GetComponent<Player>();
    }
}
