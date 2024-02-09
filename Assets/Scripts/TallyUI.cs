using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TallyUI : MonoBehaviour
{
    [SerializeField] private GameObject talleyLinePrefab;

    private Dictionary<ulong, GameObject> playerTallyUIObjects = new Dictionary<ulong, GameObject>();

    private void Start()
    {
        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
        EventContainer.GamePlay.OnPlayerScored += OnPlayerScored;

        HardRefreshScoreUI();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
        }

        EventContainer.GamePlay.OnPlayerScored -= OnPlayerScored;
    }

    private void OnConnectionEvent(NetworkManager _, ConnectionEventData connectionEventData)
    {
        HardRefreshScoreUI();
    }

    private void HardRefreshScoreUI()
    {
        ClearScoreUI();

        Player[] playersInScene = FindObjectsOfType<Player>();

        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Player player = playersInScene.Where(p => p.OwnerClientId == clientID).ToList()[0];

            UpdatePlayerScoreUI(player);
        }
    }

    private void UpdatePlayerScoreUI(Player player)
    {
        GameObject tallyLine;

        if (playerTallyUIObjects.ContainsKey(player.OwnerClientId))
        {
            tallyLine = playerTallyUIObjects[player.OwnerClientId].gameObject;
        }
        else
        {
            tallyLine = Instantiate(talleyLinePrefab, transform);
        }

        playerTallyUIObjects[player.OwnerClientId] = tallyLine;

        TextMeshProUGUI tallyText = tallyLine.GetComponentInChildren<TextMeshProUGUI>();
        tallyText.text = "Player " + (player.OwnerClientId + 1).ToString() + " Score: " + player.Score.Value;
    }

    private void ClearScoreUI()
    {
        foreach(GameObject tallyObject in playerTallyUIObjects.Values)
        {
            Destroy(tallyObject);
        }

        playerTallyUIObjects.Clear();
    }

    private void OnPlayerScored(ulong playerID, int amt)
    {
        //TODO maybe cache this a bit harder
        HardRefreshScoreUI();
    }
}
