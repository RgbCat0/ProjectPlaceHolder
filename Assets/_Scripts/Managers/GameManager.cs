using _Scripts.LobbyScripts;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public NetworkObject playerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // if (NetworkManager.IsHost)
        // {
        //     // NetworkObject.SpawnWithOwnership(0);
        // }
    }

    public void StartGame()
    {
        // spawns the players
        if (NetworkManager.IsHost)
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
            {
                // spawns in the player (the lobby player is only for lobby purposes)
                NetworkManager.SpawnManager.InstantiateAndSpawn(
                    playerPrefab,
                    (ulong)i,
                    isPlayerObject: true
                );
            }
        }
    }
}
