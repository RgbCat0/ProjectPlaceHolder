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
                        "RelayJoinCode",
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

        public async Task JoinLobbyTask(string lobbyId)
        {
            await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
        }

        public async Task<List<Lobby>> GetLobbiesAsync()
        {
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();
            return response.Results;
        }

        private void StartHeartbeat()
        {
            _heartBeatCoroutine = StartCoroutine(SendHeartbeatCoroutine());
        }

        private void StopHeartbeat()
        {
            if (_heartBeatCoroutine != null)
            {
                StopCoroutine(_heartBeatCoroutine);
                _heartBeatCoroutine = null;
            }
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
