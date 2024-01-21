using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum GameModeType
{
    Empty,
    Galaxia,
}

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameModeType gameModeTypeToLoad = GameModeType.Empty;
    [SerializeField] private AvailableGameModesSO availableGameModes;

    private GameModeBase currentGameMode;

    public GameModeBase CurrentGameMode
    {
        get { return currentGameMode; }
        set { currentGameMode = value; }
    }

    public GameModeType GameModeType
    {
        get { return gameModeTypeToLoad; }
        set { gameModeTypeToLoad = value; }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (availableGameModes == null)
        {
            Debug.LogError("Available game modes is null. Please assign it");
            return;
        }

        if (IsServer)
        {
            bool foundCorrespondingGameMode = false;

            foreach (var gameModeDataPair in availableGameModes.AvailableGameModes)
            {
                if (gameModeDataPair.GameMode == gameModeTypeToLoad)
                {
                    Debug.Log("Starting Game Mode: " + gameModeTypeToLoad.ToString());

                    GameObject gameModeManager = Instantiate(gameModeDataPair.GameModeManagerObject);
                    gameModeManager.GetComponent<NetworkObject>().Spawn();
                    currentGameMode = gameModeManager.GetComponent<GameModeBase>();

                    UpdateGameModeReferenceClientRpc();

                    return;
                }
            }

            if (!foundCorrespondingGameMode)
                Debug.LogError("GameModeType: " + gameModeTypeToLoad.ToString() + " is not in the AvailableGameModes list. Please add it to this scriptable object");
        }

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    [ClientRpc]
    private void UpdateGameModeReferenceClientRpc()
    {
        currentGameMode = FindObjectOfType<GameModeBase>();
    }
}
