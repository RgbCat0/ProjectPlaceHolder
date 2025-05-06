using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace _Scripts.LobbyScripts
{
    public class LobbyServiceManager : MonoBehaviour
    {
        public Lobby Lobby { get; private set; }
        private Coroutine _heartBeatCoroutine;

        public async Task HostLobbyTask(string lobbyName, string relayJoinCode)
        {
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "relayJoinCode",
                        new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode)
                    },
                    {
                        "HostName",
                        new DataObject(
                            DataObject.VisibilityOptions.Public,
                            AuthenticationService.Instance.PlayerName
                        )
                    },
                },
            };
            Lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, options);
            StartHeartbeat();
        }

        private void StartHeartbeat()
        {
            _heartBeatCoroutine = StartCoroutine(SendHeartbeatCoroutine());
        }

        private IEnumerator SendHeartbeatCoroutine()
        {
            while (Lobby != null)
            {
                Task heartbeatTask = LobbyService.Instance.SendHeartbeatPingAsync(Lobby.Id);
                yield return new WaitUntil(() => heartbeatTask.IsCompleted);
                if (heartbeatTask.IsFaulted)
                {
                    Debug.LogError("Failed to send heartbeat: " + heartbeatTask.Exception);
                }
                yield return new WaitForSeconds(15f);
            }
        }
    }
}
