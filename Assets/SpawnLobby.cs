using LobbyScripts;
using UnityEngine;

public class SpawnLobby : MonoBehaviour
{
    
    void Start()
    {
        LobbyController.Instance.SpawnInLobbyUI();
        Destroy(gameObject);
    }

}
