using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootScene : MonoBehaviour
{
    enum ConnectionType
    {
        Host,
        Server,
        Client
    }

    private const string sceneToLoad = "DevTest";

    private string ipAddress = "127.0.0.1";
    private string port = "7777";
    private ConnectionType connectionType;

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        // Radio buttons for Host, Client, Server
        connectionType = (ConnectionType)GUILayout.SelectionGrid((int)connectionType, new string[] { "Host", "Server", "Client" }, 3);

        // Input fields for IP and Port
        GUILayout.Label("IP Address");
        ipAddress = GUILayout.TextField(ipAddress, 25);

        GUILayout.Label("Port");
        port = GUILayout.TextField(port, 6);

        // Button to confirm the selection
        if (GUILayout.Button("Confirm"))
        {
            StartConnection(connectionType);
        }

        GUILayout.EndVertical();
    }


    public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest _, NetworkManager.ConnectionApprovalResponse response)
    {
        response.CreatePlayerObject = true;
        response.Approved = true;
    }

    private void StartConnection(ConnectionType connectionType)
    {
        Debug.Log("Starting connection...");

        string connectionData = ipAddress + ":" + port;

        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        switch (connectionType)
        {
            case ConnectionType.Server:
                Debug.Log("Attempting server creation...");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, ushort.Parse(port), "0.0.0.0");

                if (NetworkManager.Singleton.StartServer())
                {
                    Debug.Log("Server created successfully on: " + connectionData);
                }
                else
                {
                    Debug.LogError("Server creation failed on: " + connectionData);
                    return;
                }

                // Load the Game Scene in a synchronized manner using the NetworkManager
                NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
                break;

            case ConnectionType.Host:
                Debug.Log("Attempting to host...");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, ushort.Parse(port), "0.0.0.0");

                if (NetworkManager.Singleton.StartHost())
                {
                    Debug.Log("Host server created successfully on: " + connectionData);
                }
                else
                {
                    Debug.LogError("Failed to host server: " + connectionData);
                    return;
                }

                // Load the Game Scene in a synchronized manner using the NetworkManager
                NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
                break;

            case ConnectionType.Client:
                Debug.Log("Attempting to connect as client...");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, ushort.Parse(port));

                if (NetworkManager.Singleton.StartClient())
                {
                    Debug.Log("Successfully connected to: " + connectionData);
                }
                else
                {
                    Debug.LogError("Failed to connect to server: " + connectionData);
                    return;
                }

                break;
        }
    }
}
